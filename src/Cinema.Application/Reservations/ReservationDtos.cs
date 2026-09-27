namespace Cinema.Application.Reservations;

public sealed record CreateReservationRequest(Guid ShowtimeId, IReadOnlyList<Guid> SeatIds);

/// <summary>Asks the system to pick <see cref="Count"/> adjacent seats in one row.</summary>
public sealed record CreateContiguousReservationRequest(Guid ShowtimeId, int Count);

/// <summary>Customer-facing view of a reservation. Times are UTC.</summary>
public sealed record ReservationResponse(
    Guid Reference,
    ReservationState State,
    Guid ShowtimeId,
    DateTimeOffset StartTime,
    ReservationMovie Movie,
    ReservationAuditorium Auditorium,
    int SeatsCount,
    IReadOnlyList<ReservationSeat> Seats,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed record ReservationMovie(Guid Id, string Title);

public sealed record ReservationAuditorium(Guid Id, string Name);

public sealed record ReservationSeat(Guid SeatId, int Row, int Number);

/// <summary>Reservation status as seen by the customer; <see cref="Expired"/> is derived from the current time.</summary>
public enum ReservationState
{
    Pending,
    Confirmed,
    Expired
}
