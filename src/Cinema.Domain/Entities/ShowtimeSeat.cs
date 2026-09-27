using Cinema.Domain.Enums;

namespace Cinema.Domain.Entities;

/// <summary>Availability of one seat for one showtime. Composite key (ShowtimeId, SeatId).</summary>
public sealed class ShowtimeSeat
{
    private ShowtimeSeat()
    {
    }

    public Guid ShowtimeId { get; private set; }

    public Guid SeatId { get; private set; }

    public SeatStatus Status { get; private set; }

    public Guid? ReservationId { get; private set; }

    /// <summary>Optimistic concurrency token; incremented manually on every state change.</summary>
    public int Version { get; private set; }

    public static ShowtimeSeat Create(Guid showtimeId, Guid seatId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(showtimeId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(seatId, Guid.Empty);

        return new ShowtimeSeat
        {
            ShowtimeId = showtimeId,
            SeatId = seatId,
            Status = SeatStatus.Free,
            ReservationId = null,
            Version = 0
        };
    }
}
