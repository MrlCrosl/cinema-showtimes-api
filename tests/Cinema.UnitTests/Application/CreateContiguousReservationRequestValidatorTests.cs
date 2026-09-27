using Cinema.Application.Reservations;
using Shouldly;

namespace Cinema.UnitTests.Application;

public sealed class CreateContiguousReservationRequestValidatorTests
{
    private readonly CreateContiguousReservationRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(CreateReservationRequestValidator.MaxSeatsPerReservation)]
    public void Validate_CountWithinRange_Passes(int count)
    {
        // Arrange
        var request = new CreateContiguousReservationRequest(Guid.CreateVersion7(), count);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(CreateReservationRequestValidator.MaxSeatsPerReservation + 1)]
    public void Validate_CountOutOfRange_FailsOnCount(int count)
    {
        // Arrange
        var request = new CreateContiguousReservationRequest(Guid.CreateVersion7(), count);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateContiguousReservationRequest.Count));
    }

    [Fact]
    public void Validate_EmptyShowtimeId_FailsOnShowtimeId()
    {
        // Arrange
        var request = new CreateContiguousReservationRequest(Guid.Empty, 2);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateContiguousReservationRequest.ShowtimeId));
    }
}
