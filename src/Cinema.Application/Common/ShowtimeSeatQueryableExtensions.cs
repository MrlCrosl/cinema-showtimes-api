using Cinema.Application.Abstractions;
using Cinema.Domain.Entities;

namespace Cinema.Application.Common;

/// <summary>Composable query building blocks over showtime seats.</summary>
internal static class ShowtimeSeatQueryableExtensions
{
    /// <summary>Joins each showtime seat to its physical seat to expose row and number. Tracking follows the source query.</summary>
    public static IQueryable<ShowtimeSeatRow> WithPosition(this IQueryable<ShowtimeSeat> showtimeSeats, IAppDbContext dbContext) =>
        showtimeSeats.Join(
            dbContext.Seats,
            ss => ss.SeatId,
            s => s.Id,
            (ss, s) => new ShowtimeSeatRow { ShowtimeSeat = ss, Row = s.Row, Number = s.Number });
}
