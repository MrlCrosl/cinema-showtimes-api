using Cinema.Domain.Entities;

namespace Cinema.Application.Common;

/// <summary>
/// A showtime seat together with its physical position. Projection shared by the showtime and reservation features.
/// Object initializer, not a positional record: callers compose Where/OrderBy over its members after the projection,
/// and EF can only map those back to columns when the projection carries member bindings.
/// </summary>
internal sealed class ShowtimeSeatRow
{
    public required ShowtimeSeat ShowtimeSeat { get; init; }

    public required int Row { get; init; }

    public required int Number { get; init; }
}
