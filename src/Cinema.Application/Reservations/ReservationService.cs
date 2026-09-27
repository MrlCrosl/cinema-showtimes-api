using Cinema.Application.Abstractions;
using Cinema.Application.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Exceptions;
using Cinema.Domain.Seating;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Reservations;

public sealed class ReservationService(
    IAppDbContext dbContext,
    IValidator<CreateReservationRequest> validator,
    IValidator<CreateContiguousReservationRequest> contiguousValidator,
    TimeProvider timeProvider) : IReservationService
{
    private const string ConcurrencyConflictMessage = "One or more seats were taken by another request. Please retry.";

    public async Task<ReservationResponse> ReserveAsync(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var showtime = await LoadUpcomingShowtimeAsync(request.ShowtimeId, now, cancellationToken);

        // Tracked: these rows are updated below and their Version guards the read-then-write.
        var requestedSeatIds = request.SeatIds.ToList();
        var seats = await dbContext.ShowtimeSeats
            .Where(ss => ss.ShowtimeId == showtime.Id && requestedSeatIds.Contains(ss.SeatId))
            .WithPosition(dbContext)
            .ToListAsync(cancellationToken);

        var unknownSeatIds = requestedSeatIds.Except(seats.Select(x => x.ShowtimeSeat.SeatId)).ToList();
        if (unknownSeatIds.Count > 0)
        {
            throw new DomainValidationException(
                $"Seats not found for showtime '{showtime.Id}': {string.Join(", ", unknownSeatIds)}.");
        }

        var holders = await dbContext.LoadHoldersAsync(seats.Select(x => x.ShowtimeSeat), cancellationToken);

        var unavailable = seats
            .Where(x => !x.ShowtimeSeat.IsAvailable(holders.HolderOf(x.ShowtimeSeat), now))
            .OrderBy(x => x.Row).ThenBy(x => x.Number)
            .Select(x => $"row {x.Row} seat {x.Number}")
            .ToList();

        if (unavailable.Count > 0)
        {
            throw new ConflictException($"The following seats are not available: {string.Join(", ", unavailable)}.");
        }

        return await ReserveSeatsAsync(showtime.Id, seats, now, cancellationToken);
    }

    public async Task<ReservationResponse> ReserveContiguousAsync(CreateContiguousReservationRequest request, CancellationToken cancellationToken = default)
    {
        await contiguousValidator.ValidateAndThrowAsync(request, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var showtime = await LoadUpcomingShowtimeAsync(request.ShowtimeId, now, cancellationToken);

        // Whole seat map is loaded tracked; only the chosen seats are modified, so the Version check still applies.
        var seats = await dbContext.ShowtimeSeats
            .Where(ss => ss.ShowtimeId == showtime.Id)
            .WithPosition(dbContext)
            .ToListAsync(cancellationToken);

        var holders = await dbContext.LoadHoldersAsync(seats.Select(x => x.ShowtimeSeat), cancellationToken);

        var available = seats
            .Where(x => x.ShowtimeSeat.IsAvailable(holders.HolderOf(x.ShowtimeSeat), now))
            .Select(x => new SeatPosition(x.ShowtimeSeat.SeatId, x.Row, x.Number));

        var block = ContiguousSeatFinder.FindBlock(available, request.Count)
            ?? throw new ConflictException($"No contiguous block of {request.Count} seats is available.");

        var chosen = seats.Where(x => block.Contains(x.ShowtimeSeat.SeatId)).ToList();

        return await ReserveSeatsAsync(showtime.Id, chosen, now, cancellationToken);
    }

    public async Task<ReservationResponse> ConfirmAsync(Guid reference, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var reservation = await dbContext.Reservations
            .SingleOrDefaultAsync(r => r.Id == reference, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), reference);

        var seats = await dbContext.ShowtimeSeats
            .Where(ss => ss.ReservationId == reference)
            .ToListAsync(cancellationToken);

        reservation.Confirm(now);
        foreach (var seat in seats)
        {
            seat.Sell();
        }

        await SaveOrConflictAsync(cancellationToken);

        return await GetResponseAsync(reference, now, cancellationToken);
    }

    public async Task<ReservationResponse> GetAsync(Guid reference, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await GetResponseAsync(reference, now, cancellationToken);
    }

    /// <exception cref="NotFoundException">The showtime does not exist.</exception>
    /// <exception cref="ConflictException">The showtime has already started.</exception>
    private async Task<Showtime> LoadUpcomingShowtimeAsync(Guid showtimeId, DateTime now, CancellationToken cancellationToken)
    {
        var showtime = await dbContext.Showtimes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == showtimeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Showtime), showtimeId);

        if (now >= showtime.StartTime)
        {
            throw new ConflictException($"Showtime '{showtime.Id}' already started at {showtime.StartTime:O}.");
        }

        return showtime;
    }

    /// <summary>
    /// Creates the reservation and moves the (already availability-checked, tracked) seats to Reserved in one SaveChanges.
    /// The seats' Version token turns a lost race into <see cref="ConflictException"/>.
    /// </summary>
    private async Task<ReservationResponse> ReserveSeatsAsync(
        Guid showtimeId,
        IReadOnlyCollection<ShowtimeSeatRow> seats,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var reservation = Reservation.Create(showtimeId, now);
        foreach (var seat in seats)
        {
            seat.ShowtimeSeat.Reserve(reservation.Id);
        }

        dbContext.Reservations.Add(reservation);
        await SaveOrConflictAsync(cancellationToken);

        return await GetResponseAsync(reservation.Id, now, cancellationToken);
    }

    private async Task<ReservationResponse> GetResponseAsync(Guid reference, DateTime now, CancellationToken cancellationToken)
    {
        var header = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.Id == reference)
            .Join(dbContext.Showtimes, r => r.ShowtimeId, s => s.Id, (r, s) => new { Reservation = r, Showtime = s })
            .Join(dbContext.Movies, x => x.Showtime.MovieId, m => m.Id, (x, m) => new { x.Reservation, x.Showtime, Movie = m })
            .Join(dbContext.Auditoriums, x => x.Showtime.AuditoriumId, a => a.Id, (x, a) => new ReservationRow
            {
                Reservation = x.Reservation,
                StartTime = x.Showtime.StartTime,
                MovieId = x.Movie.Id,
                MovieTitle = x.Movie.Title,
                AuditoriumId = a.Id,
                AuditoriumName = a.Name
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), reference);

        var seats = await dbContext.ShowtimeSeats
            .AsNoTracking()
            .Where(ss => ss.ReservationId == reference)
            .Join(dbContext.Seats, ss => ss.SeatId, s => s.Id, (ss, s) => s)
            .OrderBy(s => s.Row).ThenBy(s => s.Number)
            .Select(s => new ReservationSeat(s.Id, s.Row, s.Number))
            .ToListAsync(cancellationToken);

        var reservation = header.Reservation;

        return new ReservationResponse(
            reservation.Id,
            StateOf(reservation, now),
            reservation.ShowtimeId,
            new DateTimeOffset(header.StartTime, TimeSpan.Zero),
            new ReservationMovie(header.MovieId, header.MovieTitle),
            new ReservationAuditorium(header.AuditoriumId, header.AuditoriumName),
            seats.Count,
            seats,
            new DateTimeOffset(reservation.CreatedAt, TimeSpan.Zero),
            new DateTimeOffset(reservation.ExpiresAt, TimeSpan.Zero));
    }

    private static ReservationState StateOf(Reservation reservation, DateTime now)
    {
        if (reservation.Status == ReservationStatus.Confirmed)
        {
            return ReservationState.Confirmed;
        }

        return reservation.IsExpired(now) ? ReservationState.Expired : ReservationState.Pending;
    }

    private async Task SaveOrConflictAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(ConcurrencyConflictMessage);
        }
    }

    private sealed class ReservationRow
    {
        public required Reservation Reservation { get; init; }

        public required DateTime StartTime { get; init; }

        public required Guid MovieId { get; init; }

        public required string MovieTitle { get; init; }

        public required Guid AuditoriumId { get; init; }

        public required string AuditoriumName { get; init; }
    }
}
