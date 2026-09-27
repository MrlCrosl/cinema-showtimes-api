using Cinema.Application.Abstractions;
using Cinema.Application.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Showtimes;

public sealed class ShowtimeService(
    IAppDbContext dbContext,
    IValidator<CreateShowtimeRequest> validator,
    TimeProvider timeProvider) : IShowtimeService
{
    public async Task<ShowtimeResponse> CreateAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var movie = await dbContext.Movies
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken)
            ?? throw new NotFoundException(nameof(Movie), request.MovieId);

        var auditorium = await dbContext.Auditoriums
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == request.AuditoriumId, cancellationToken)
            ?? throw new NotFoundException(nameof(Auditorium), request.AuditoriumId);

        var start = request.StartTime.UtcDateTime;
        var end = start.AddMinutes(movie.DurationMinutes);

        await EnsureNoOverlapAsync(auditorium.Id, start, end, cancellationToken);

        var seatIds = await dbContext.Seats
            .AsNoTracking()
            .Where(s => s.AuditoriumId == auditorium.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var showtime = Showtime.Create(movie.Id, auditorium.Id, start);
        var showtimeSeats = seatIds.Select(seatId => ShowtimeSeat.Create(showtime.Id, seatId));

        dbContext.Showtimes.Add(showtime);
        dbContext.ShowtimeSeats.AddRange(showtimeSeats);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(showtime, movie.Title, movie.DurationMinutes, auditorium.Name);
    }

    public async Task<IReadOnlyList<ShowtimeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await QueryWithNames()
            .OrderBy(x => x.Showtime.StartTime)
            .ToListAsync(cancellationToken);

        return rows.Select(x => ToResponse(x.Showtime, x.MovieTitle, x.DurationMinutes, x.AuditoriumName)).ToList();
    }

    public async Task<ShowtimeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await QueryWithNames()
            .SingleOrDefaultAsync(x => x.Showtime.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Showtime), id);

        return ToResponse(row.Showtime, row.MovieTitle, row.DurationMinutes, row.AuditoriumName);
    }

    public async Task<IReadOnlyList<SeatAvailabilityResponse>> GetSeatsAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var exists = await dbContext.Showtimes
            .AsNoTracking()
            .AnyAsync(s => s.Id == showtimeId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(nameof(Showtime), showtimeId);
        }

        var seats = await dbContext.ShowtimeSeats
            .AsNoTracking()
            .Where(ss => ss.ShowtimeId == showtimeId)
            .WithPosition(dbContext)
            .OrderBy(x => x.Row).ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);

        var holders = await dbContext.LoadHoldersAsync(seats.Select(x => x.ShowtimeSeat), cancellationToken);

        return seats
            .Select(x => new SeatAvailabilityResponse(
                x.ShowtimeSeat.SeatId,
                x.Row,
                x.Number,
                EffectiveStatus(x.ShowtimeSeat, holders.HolderOf(x.ShowtimeSeat), now)))
            .ToList();
    }

    /// <summary>Reports a seat as Free when it can be reserved now, which covers reserved seats whose hold has expired.</summary>
    private static SeatStatus EffectiveStatus(ShowtimeSeat seat, Reservation? holder, DateTime now) =>
        seat.IsAvailable(holder, now) ? SeatStatus.Free : seat.Status;

    /// <summary>
    /// Rejects the new interval [start, end) if any showtime in the same auditorium overlaps it.
    /// Candidates are narrowed by the (AuditoriumId, StartTime) index: an existing showtime can only overlap
    /// if it starts before <paramref name="end"/> and no earlier than the longest possible movie before <paramref name="start"/>.
    /// </summary>
    private async Task EnsureNoOverlapAsync(Guid auditoriumId, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var windowStart = start.AddMinutes(-Movie.MaxDurationMinutes);

        var candidates = await dbContext.Showtimes
            .AsNoTracking()
            .Where(s => s.AuditoriumId == auditoriumId && s.StartTime >= windowStart && s.StartTime < end)
            .Join(dbContext.Movies, s => s.MovieId, m => m.Id, (s, m) => new { s.StartTime, m.DurationMinutes })
            .ToListAsync(cancellationToken);

        var overlapping = candidates.FirstOrDefault(c =>
            c.StartTime < end && c.StartTime.AddMinutes(c.DurationMinutes) > start);

        if (overlapping is not null)
        {
            throw new ConflictException(
                $"The showtime overlaps another showtime in the same auditorium starting at {overlapping.StartTime:O}.");
        }
    }

    private IQueryable<ShowtimeRow> QueryWithNames() =>
        dbContext.Showtimes
            .AsNoTracking()
            .Join(dbContext.Movies, s => s.MovieId, m => m.Id, (s, m) => new { Showtime = s, Movie = m })
            .Join(dbContext.Auditoriums, x => x.Showtime.AuditoriumId, a => a.Id,
                (x, a) => new ShowtimeRow
                {
                    Showtime = x.Showtime,
                    MovieTitle = x.Movie.Title,
                    DurationMinutes = x.Movie.DurationMinutes,
                    AuditoriumName = a.Name
                });

    private static ShowtimeResponse ToResponse(Showtime showtime, string movieTitle, int durationMinutes, string auditoriumName)
    {
        var start = new DateTimeOffset(showtime.StartTime, TimeSpan.Zero);

        return new ShowtimeResponse(
            showtime.Id,
            showtime.MovieId,
            movieTitle,
            showtime.AuditoriumId,
            auditoriumName,
            start,
            start.AddMinutes(durationMinutes));
    }

    /// <summary>Query projection. Uses an object initializer (not a positional ctor) so EF can compose Where/OrderBy over its members.</summary>
    private sealed class ShowtimeRow
    {
        public required Showtime Showtime { get; init; }

        public required string MovieTitle { get; init; }

        public required int DurationMinutes { get; init; }

        public required string AuditoriumName { get; init; }
    }
}
