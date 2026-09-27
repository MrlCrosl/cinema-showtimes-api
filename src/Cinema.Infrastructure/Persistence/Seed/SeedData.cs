using System.Globalization;

namespace Cinema.Infrastructure.Persistence.Seed;

/// <summary>
/// Deterministic seed data applied through <c>HasData</c>. Ids are fixed so migrations stay stable.
/// Entities have private constructors, so rows are described as anonymous objects (supported by HasData).
/// </summary>
public static class SeedData
{
    public static readonly Guid Hall1Id = new("0d3a1b6e-1c4f-4a2d-9b7e-000000000001");

    public const int Hall1Rows = 5;

    public const int Hall1SeatsPerRow = 8;

    public static readonly Guid InceptionId = new("6d1e2f3a-4b5c-4d6e-8f90-000000000001");
    public static readonly Guid TheGodfatherId = new("6d1e2f3a-4b5c-4d6e-8f90-000000000002");
    public static readonly Guid SpiritedAwayId = new("6d1e2f3a-4b5c-4d6e-8f90-000000000003");
    public static readonly Guid TheGrandBudapestHotelId = new("6d1e2f3a-4b5c-4d6e-8f90-000000000004");
    public static readonly Guid MadMaxFuryRoadId = new("6d1e2f3a-4b5c-4d6e-8f90-000000000005");

    public static object[] Auditoriums =>
    [
        new { Id = Hall1Id, Name = "Hall 1" }
    ];

    public static object[] Movies =>
    [
        new { Id = InceptionId, Title = "Inception", Category = "Sci-Fi", Year = 2010, DurationMinutes = 148 },
        new { Id = TheGodfatherId, Title = "The Godfather", Category = "Crime", Year = 1972, DurationMinutes = 175 },
        new { Id = SpiritedAwayId, Title = "Spirited Away", Category = "Animation", Year = 2001, DurationMinutes = 125 },
        new { Id = TheGrandBudapestHotelId, Title = "The Grand Budapest Hotel", Category = "Comedy", Year = 2014, DurationMinutes = 99 },
        new { Id = MadMaxFuryRoadId, Title = "Mad Max: Fury Road", Category = "Action", Year = 2015, DurationMinutes = 120 }
    ];

    public static object[] Seats =>
        Enumerable.Range(1, Hall1Rows)
            .SelectMany(row => Enumerable.Range(1, Hall1SeatsPerRow)
                .Select(number => (object)new { Id = SeatId(Hall1Id, row, number), AuditoriumId = Hall1Id, Row = row, Number = number }))
            .ToArray();

    /// <summary>Derives a stable seat id from the auditorium and the seat position.</summary>
    public static Guid SeatId(Guid auditoriumId, int row, int number)
    {
        // Keep the first 20 hex digits of the auditorium id and encode row/number in the last 12.
        var prefix = auditoriumId.ToString("N")[..20];
        var suffix = string.Create(CultureInfo.InvariantCulture, $"{row:D6}{number:D6}");
        return Guid.ParseExact(prefix + suffix, "N");
    }
}
