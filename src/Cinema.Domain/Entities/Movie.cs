namespace Cinema.Domain.Entities;

public sealed class Movie
{
    /// <summary>Year the first motion picture was made; used as a lower bound for <see cref="Year"/>.</summary>
    public const int MinYear = 1888;

    private Movie()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public int DurationMinutes { get; private set; }

    public static Movie Create(string title, string category, int year, int durationMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentOutOfRangeException.ThrowIfLessThan(year, MinYear);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);

        return new Movie
        {
            Id = Guid.CreateVersion7(),
            Title = title.Trim(),
            Category = category.Trim(),
            Year = year,
            DurationMinutes = durationMinutes
        };
    }
}
