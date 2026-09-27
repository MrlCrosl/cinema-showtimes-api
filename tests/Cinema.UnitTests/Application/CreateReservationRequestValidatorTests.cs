using Cinema.Application.Reservations;
using Shouldly;

namespace Cinema.UnitTests.Application;

public sealed class CreateReservationRequestValidatorTests
{
    private readonly CreateReservationRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        // Arrange
        var request = new CreateReservationRequest(Guid.CreateVersion7(), NewSeatIds(2));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_MaximumNumberOfSeats_Passes()
    {
        // Arrange
        var request = new CreateReservationRequest(
            Guid.CreateVersion7(),
            NewSeatIds(CreateReservationRequestValidator.MaxSeatsPerReservation));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyShowtimeId_FailsOnShowtimeId()
    {
        // Arrange
        var request = new CreateReservationRequest(Guid.Empty, NewSeatIds(1));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateReservationRequest.ShowtimeId));
    }

    public static TheoryData<string, IReadOnlyList<Guid>> InvalidSeatIds => new()
    {
        { "empty list", [] },
        { "more than the maximum", NewSeatIds(CreateReservationRequestValidator.MaxSeatsPerReservation + 1) },
        { "duplicates", DuplicateSeatIds() },
        { "empty guid", [Guid.CreateVersion7(), Guid.Empty] }
    };

    [Theory]
    [MemberData(nameof(InvalidSeatIds))]
    public void Validate_InvalidSeatIds_FailsOnSeatIds(string scenario, IReadOnlyList<Guid> seatIds)
    {
        // Arrange
        var request = new CreateReservationRequest(Guid.CreateVersion7(), seatIds);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse(scenario);
        result.Errors.ShouldAllBe(e => e.PropertyName.StartsWith(nameof(CreateReservationRequest.SeatIds)), scenario);
    }

    [Fact]
    public void Validate_EmptyList_ReportsSingleError()
    {
        // Arrange
        var request = new CreateReservationRequest(Guid.CreateVersion7(), []);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Count.ShouldBe(1);
    }

    private static List<Guid> NewSeatIds(int count) =>
        Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7()).ToList();

    private static List<Guid> DuplicateSeatIds()
    {
        var id = Guid.CreateVersion7();
        return [id, Guid.CreateVersion7(), id];
    }
}
