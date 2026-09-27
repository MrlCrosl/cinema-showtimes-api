using Cinema.Application.Abstractions;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Common;

/// <summary>Reads that several features need from the unit of work.</summary>
internal static class AppDbContextExtensions
{
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
}
