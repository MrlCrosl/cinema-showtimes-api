using Cinema.Domain.Enums;

namespace Cinema.Domain.Entities;

public sealed class Reservation
{
    /// <summary>How long a pending reservation holds its seats before it expires.</summary>
    public static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

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
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Current time must be in UTC.", nameof(utcNow));
        }

        return new Reservation
        {
            Id = Guid.CreateVersion7(),
            ShowtimeId = showtimeId,
            CreatedAt = utcNow,
            ExpiresAt = utcNow + HoldDuration,
            Status = ReservationStatus.Pending
        };
    }
}
