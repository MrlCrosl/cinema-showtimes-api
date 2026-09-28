using Cinema.Application.Showtimes;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Cinema.UnitTests.Application;

public sealed class CreateShowtimeRequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly CreateShowtimeRequestValidator _validator = new(new FakeTimeProvider(Now));

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        // Arrange
        var request = new CreateShowtimeRequest(Guid.CreateVersion7(), Guid.CreateVersion7(), Now.AddHours(1));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyMovieId_FailsOnMovieId()
    {
        // Arrange
        var request = new CreateShowtimeRequest(Guid.Empty, Guid.CreateVersion7(), Now.AddHours(1));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateShowtimeRequest.MovieId));
    }

    [Fact]
    public void Validate_EmptyAuditoriumId_FailsOnAuditoriumId()
    {
        // Arrange
        var request = new CreateShowtimeRequest(Guid.CreateVersion7(), Guid.Empty, Now.AddHours(1));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateShowtimeRequest.AuditoriumId));
    }

    [Fact]
    public void Validate_StartTimeNotInFuture_FailsOnStartTime()
    {
        // Arrange
        var request = new CreateShowtimeRequest(Guid.CreateVersion7(), Guid.CreateVersion7(), Now);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(CreateShowtimeRequest.StartTime));
    }
}
