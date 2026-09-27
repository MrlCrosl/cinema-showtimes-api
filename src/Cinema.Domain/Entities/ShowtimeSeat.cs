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

    /// <summary>
    /// Whether the seat can be reserved now. A reserved seat becomes available again once the reservation
    /// holding it (<paramref name="holder"/>, looked up by <see cref="ReservationId"/>) has expired.
    /// </summary>
    public bool IsAvailable(Reservation? holder, DateTime utcNow) => Status switch
    {
        SeatStatus.Free => true,
        SeatStatus.Sold => false,
        SeatStatus.Reserved => holder is not null && holder.IsExpired(utcNow),
        _ => false
    };

    /// <exception cref="InvalidOperationException">The seat is already sold.</exception>
    public void Reserve(Guid reservationId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(reservationId, Guid.Empty);

        if (Status == SeatStatus.Sold)
        {
            throw new InvalidOperationException($"Seat '{SeatId}' for showtime '{ShowtimeId}' is already sold.");
        }

        Status = SeatStatus.Reserved;
        ReservationId = reservationId;
        Version++;
    }

    /// <exception cref="InvalidOperationException">The seat is not currently reserved.</exception>
    public void Sell()
    {
        if (Status != SeatStatus.Reserved)
        {
            throw new InvalidOperationException($"Seat '{SeatId}' for showtime '{ShowtimeId}' must be reserved before it can be sold; it is {Status}.");
        }

        Status = SeatStatus.Sold;
        Version++;
    }
}
