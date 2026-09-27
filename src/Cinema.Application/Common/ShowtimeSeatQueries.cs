using Cinema.Application.Abstractions;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Common;

/// <summary>Shared building blocks for working with showtime seats and the reservations holding them.</summary>
internal static class ShowtimeSeatQueries
{
    /// <summary>Joins each showtime seat to its physical seat to expose row and number. Tracking follows the source query.</summary>
    public static IQueryable<ShowtimeSeatRow> WithPosition(this IQueryable<ShowtimeSeat> showtimeSeats, IAppDbContext dbContext) =>
        showtimeSeats.Join(
            dbContext.Seats,
            ss => ss.SeatId,
            s => s.Id,
            (ss, s) => new ShowtimeSeatRow { ShowtimeSeat = ss, Row = s.Row, Number = s.Number });

    /// <summary>Loads, in one untracked query, the reservations referenced by the given seats, keyed by reservation id.</summary>
    public static async Task<Dictionary<Guid, Reservation>> LoadHoldersAsync(
        this IAppDbContext dbContext,
        IEnumerable<ShowtimeSeat> seats,
        CancellationToken cancellationToken)
    {
        var holderIds = seats
            .Where(ss => ss.ReservationId is not null)
            .Select(ss => ss.ReservationId!.Value)
            .Distinct()
            .ToList();

        if (holderIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => holderIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);
    }

    /// <summary>The reservation currently holding the seat, or null when the seat is free or its holder is unknown.</summary>
    public static Reservation? HolderOf(this IReadOnlyDictionary<Guid, Reservation> holders, ShowtimeSeat seat) =>
        seat.ReservationId is { } id && holders.TryGetValue(id, out var holder) ? holder : null;
}
