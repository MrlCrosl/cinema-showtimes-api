namespace Cinema.Domain.Seating;

/// <summary>
/// Finds a block of adjacent seats among the seats that are currently available. Pure and deterministic:
/// rows are scanned ascending, numbers ascending, and the leftmost fitting block in the lowest row wins (first fit).
/// A block never spans rows, and seat numbers must be consecutive, so any unavailable seat in between breaks a block.
/// </summary>
public static class ContiguousSeatFinder
{
    /// <returns>The seat ids of the chosen block in number order, or null when no row has such a block.</returns>
    public static IReadOnlyList<Guid>? FindBlock(IEnumerable<SeatPosition> availableSeats, int count)
    {
        ArgumentNullException.ThrowIfNull(availableSeats);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var rows = availableSeats
            .GroupBy(s => s.Row)
            .OrderBy(g => g.Key);

        foreach (var row in rows)
        {
            var run = new List<Guid>(count);
            var previousNumber = int.MinValue;

            foreach (var seat in row.OrderBy(s => s.Number))
            {
                if (seat.Number != previousNumber + 1)
                {
                    run.Clear();
                }

                run.Add(seat.SeatId);
                previousNumber = seat.Number;

                if (run.Count == count)
                {
                    return run;
                }
            }
        }

        return null;
    }
}
