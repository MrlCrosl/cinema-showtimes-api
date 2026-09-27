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
