using Cinema.Domain.Enums;
using Cinema.Domain.Exceptions;

namespace Cinema.Domain.Entities;

public sealed class Reservation
{
    /// <summary>How long a pending reservation holds its seats before it expires.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private Reservation()
    {
    }

    /// <summary>Reservation id; doubles as the customer-facing reference.</summary>
    public Guid Id { get; private set; }

    public Guid ShowtimeId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public ReservationStatus Status { get; private set; }

    public static Reservation Create(Guid showtimeId, DateTime utcNow)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(showtimeId, Guid.Empty);
        EnsureUtc(utcNow);

        return new Reservation
        {
            Id = Guid.CreateVersion7(),
            ShowtimeId = showtimeId,
            CreatedAt = utcNow,
            ExpiresAt = utcNow + Lifetime,
            Status = ReservationStatus.Pending
        };
    }

    /// <summary>The single expiry rule: a pending reservation is expired once <paramref name="utcNow"/> reaches <see cref="ExpiresAt"/>.</summary>
    public bool IsExpired(DateTime utcNow) => Status == ReservationStatus.Pending && utcNow >= ExpiresAt;

    /// <exception cref="ConflictException">The reservation is already confirmed or has expired.</exception>
    public void Confirm(DateTime utcNow)
    {
        EnsureUtc(utcNow);

        if (Status == ReservationStatus.Confirmed)
        {
            throw new ConflictException($"Reservation '{Id}' is already confirmed.");
        }

        if (IsExpired(utcNow))
        {
            throw new ConflictException($"Reservation '{Id}' expired at {ExpiresAt:O}.");
        }

        Status = ReservationStatus.Confirmed;
    }

    private static void EnsureUtc(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Current time must be in UTC.", nameof(utcNow));
        }
    }
}
