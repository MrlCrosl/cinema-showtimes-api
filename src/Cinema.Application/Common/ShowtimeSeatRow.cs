using Cinema.Domain.Entities;

namespace Cinema.Application.Common;

/// <summary>
/// A showtime seat together with its physical position. Query projection: uses an object initializer
/// (not a positional constructor) so EF can compose Where/OrderBy over its members.
/// </summary>
internal sealed class ShowtimeSeatRow
{
    public required ShowtimeSeat ShowtimeSeat { get; init; }

    public required int Row { get; init; }

    public required int Number { get; init; }
}
