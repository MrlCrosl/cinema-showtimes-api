using Cinema.Domain.Entities;

namespace Cinema.Application.Common;

/// <summary>Lookups over the reservations holding a showtime's seats, as loaded by <see cref="AppDbContextExtensions.LoadHoldersAsync"/>.</summary>
internal static class ReservationHoldersExtensions
{
    /// <summary>The reservation currently holding the seat, or null when the seat is free or its holder is unknown.</summary>
    public static Reservation? HolderOf(this IReadOnlyDictionary<Guid, Reservation> holders, ShowtimeSeat seat) =>
        seat.ReservationId is { } id && holders.TryGetValue(id, out var holder) ? holder : null;
}
