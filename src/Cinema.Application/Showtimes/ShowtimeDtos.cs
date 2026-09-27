using Cinema.Domain.Enums;

namespace Cinema.Application.Showtimes;

public sealed record CreateShowtimeRequest(Guid MovieId, Guid AuditoriumId, DateTimeOffset StartTime);

/// <summary>Showtime with its movie and auditorium names. <see cref="StartTime"/> and <see cref="EndTime"/> are UTC.</summary>
public sealed record ShowtimeResponse(
    Guid Id,
    Guid MovieId,
    string MovieTitle,
    Guid AuditoriumId,
    string AuditoriumName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);

/// <summary>Availability of one seat for a showtime. A reserved seat whose reservation expired is reported as <see cref="SeatStatus.Free"/>.</summary>
public sealed record SeatAvailabilityResponse(Guid SeatId, int Row, int Number, SeatStatus Status);
