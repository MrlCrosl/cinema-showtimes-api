namespace Cinema.Domain.Seating;

/// <summary>A seat's physical position, the only input <see cref="ContiguousSeatFinder"/> needs.</summary>
public readonly record struct SeatPosition(Guid SeatId, int Row, int Number);
