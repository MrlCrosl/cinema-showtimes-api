namespace Cinema.Domain.Entities;

public sealed class Showtime
{
    private Showtime()
    {
    }

    public Guid Id { get; private set; }

    public Guid MovieId { get; private set; }

    public Guid AuditoriumId { get; private set; }

    /// <summary>Start of the showtime in UTC.</summary>
    public DateTime StartTime { get; private set; }

    public static Showtime Create(Guid movieId, Guid auditoriumId, DateTime startTimeUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(movieId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(auditoriumId, Guid.Empty);
        if (startTimeUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Start time must be in UTC.", nameof(startTimeUtc));
        }

        return new Showtime
        {
            Id = Guid.CreateVersion7(),
            MovieId = movieId,
            AuditoriumId = auditoriumId,
            StartTime = startTimeUtc
        };
    }
}
