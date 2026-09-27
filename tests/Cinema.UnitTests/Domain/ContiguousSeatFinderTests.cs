using Cinema.Domain.Seating;
using Shouldly;

namespace Cinema.UnitTests.Domain;

public sealed class ContiguousSeatFinderTests
{
    [Fact]
    public void FindBlock_BlockAvailableInFirstRow_ReturnsLeftmostSeatsOfFirstRow()
    {
        // Arrange
        var seats = Grid(rows: 2, seatsPerRow: 5);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 3);

        // Assert
        block.ShouldBe(Ids(seats, (1, 1), (1, 2), (1, 3)));
    }

    [Fact]
    public void FindBlock_FirstRowTooFragmented_ReturnsBlockInLaterRow()
    {
        // Arrange
        var seats = Grid(rows: 2, seatsPerRow: 5, unavailable: [(1, 2), (1, 4)]);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 2);

        // Assert
        block.ShouldBe(Ids(seats, (2, 1), (2, 2)));
    }

    [Fact]
    public void FindBlock_GapInMiddleOfRow_SkipsSeatsBeforeGap()
    {
        // Arrange
        var seats = Grid(rows: 1, seatsPerRow: 6, unavailable: [(1, 3)]);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 3);

        // Assert
        block.ShouldBe(Ids(seats, (1, 4), (1, 5), (1, 6)));
    }

    [Fact]
    public void FindBlock_BlockWouldSpanTwoRows_ReturnsNull()
    {
        // Arrange: seats 1-4 in row 1 and 1-4 in row 2 (eight seats, no row has five)
        var seats = Grid(rows: 2, seatsPerRow: 4);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 5);

        // Assert
        block.ShouldBeNull();
    }

    [Fact]
    public void FindBlock_CountLargerThanAnyRow_ReturnsNull()
    {
        // Arrange
        var seats = Grid(rows: 3, seatsPerRow: 3);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 4);

        // Assert
        block.ShouldBeNull();
    }

    [Fact]
    public void FindBlock_CountOne_ReturnsFirstAvailableSeat()
    {
        // Arrange
        var seats = Grid(rows: 2, seatsPerRow: 3, unavailable: [(1, 1), (1, 2)]);

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 1);

        // Assert
        block.ShouldBe(Ids(seats, (1, 3)));
    }

    [Fact]
    public void FindBlock_SeveralRowsFit_PicksLowestRow()
    {
        // Arrange: rows are supplied out of order to prove ordering is not input-dependent
        var seats = Grid(rows: 3, seatsPerRow: 4, unavailable: [(1, 1)]).AsEnumerable().Reverse().ToList();

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 2);

        // Assert
        block.ShouldBe(Ids(seats, (1, 2), (1, 3)));
    }

    [Fact]
    public void FindBlock_SeatsSuppliedUnordered_StillReturnsLeftmostBlock()
    {
        // Arrange
        var seats = Grid(rows: 1, seatsPerRow: 5).OrderByDescending(s => s.Number).ToList();

        // Act
        var block = ContiguousSeatFinder.FindBlock(seats, 2);

        // Assert
        block.ShouldBe(Ids(seats, (1, 1), (1, 2)));
    }

    [Fact]
    public void FindBlock_NoSeats_ReturnsNull()
    {
        // Act
        var block = ContiguousSeatFinder.FindBlock([], 1);

        // Assert
        block.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FindBlock_CountBelowOne_ThrowsArgumentOutOfRangeException(int count)
    {
        // Arrange
        var seats = Grid(rows: 1, seatsPerRow: 3);

        // Act
        var act = () => ContiguousSeatFinder.FindBlock(seats, count);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    /// <summary>All seats of a rectangular grid except the unavailable positions.</summary>
    private static List<SeatPosition> Grid(int rows, int seatsPerRow, (int Row, int Number)[]? unavailable = null)
    {
        var blocked = (unavailable ?? []).ToHashSet();

        return Enumerable.Range(1, rows)
            .SelectMany(row => Enumerable.Range(1, seatsPerRow).Select(number => (Row: row, Number: number)))
            .Where(p => !blocked.Contains(p))
            .Select(p => new SeatPosition(Guid.CreateVersion7(), p.Row, p.Number))
            .ToList();
    }

    private static List<Guid> Ids(IEnumerable<SeatPosition> seats, params (int Row, int Number)[] positions) =>
        positions.Select(p => seats.Single(s => s.Row == p.Row && s.Number == p.Number).SeatId).ToList();
}
