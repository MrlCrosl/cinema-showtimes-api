using Cinema.Application.Movies;
using Cinema.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Cinema.UnitTests.Application;

public sealed class CreateMovieRequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly CreateMovieRequestValidator _validator = new(new FakeTimeProvider(Now));

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        // Arrange
        var request = new CreateMovieRequest("Heat", "Crime", 1995, 170);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    public static TheoryData<string, CreateMovieRequest, string> InvalidRequests => new()
    {
        { "empty title", new CreateMovieRequest("", "Crime", 1995, 170), nameof(CreateMovieRequest.Title) },
        { "title too long", new CreateMovieRequest(new string('a', 201), "Crime", 1995, 170), nameof(CreateMovieRequest.Title) },
        { "empty category", new CreateMovieRequest("Heat", "", 1995, 170), nameof(CreateMovieRequest.Category) },
        { "category too long", new CreateMovieRequest("Heat", new string('a', 101), 1995, 170), nameof(CreateMovieRequest.Category) },
        { "year before first film", new CreateMovieRequest("Heat", "Crime", Movie.MinYear - 1, 170), nameof(CreateMovieRequest.Year) },
        {
            "year too far ahead",
            new CreateMovieRequest("Heat", "Crime", Now.Year + CreateMovieRequestValidator.MaxYearsAhead + 1, 170),
            nameof(CreateMovieRequest.Year)
        },
        { "zero duration", new CreateMovieRequest("Heat", "Crime", 1995, 0), nameof(CreateMovieRequest.DurationMinutes) },
        {
            "duration too long",
            new CreateMovieRequest("Heat", "Crime", 1995, Movie.MaxDurationMinutes + 1),
            nameof(CreateMovieRequest.DurationMinutes)
        }
    };

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public void Validate_InvalidField_FailsOnThatField(string scenario, CreateMovieRequest request, string property)
    {
        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse(scenario);
        result.Errors.ShouldAllBe(e => e.PropertyName == property, scenario);
    }
}
