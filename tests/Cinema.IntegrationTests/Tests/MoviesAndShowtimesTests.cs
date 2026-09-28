using System.Globalization;
using Cinema.Application.Movies;
using Cinema.Infrastructure.Persistence.Seed;
using Cinema.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class MoviesAndShowtimesTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateMovie_ValidRequest_Returns201AndIsRetrievable()
    {
        // Arrange
        var request = new CreateMovieRequest("Dune: Part Two", "Sci-Fi", 2024, 166);

        // Act
        var response = await PostMovieAsync(request);
        var created = await ReadAsync<MovieResponse>(response);
        var fetched = await ReadAsync<MovieResponse>(await Client.GetAsync($"/api/movies/{created.Id}"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.ToString().ShouldEndWith($"/api/movies/{created.Id}");
        fetched.ShouldBe(created);
        created.ShouldBe(new MovieResponse(created.Id, "Dune: Part Two", "Sci-Fi", 2024, 166));
    }

    [Fact]
    public async Task CreateMovie_InvalidYear_Returns400WithYearError()
    {
        // Arrange
        var request = new CreateMovieRequest("Old", "Drama", 1800, 90);

        // Act
        var response = await PostMovieAsync(request);
        var problem = await ReadAsync<ValidationProblemDetails>(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Keys.ShouldBe(["year"]);
    }

    [Fact]
    public async Task CreateMovie_NonEnglishUICulture_Returns400WithEnglishMessage()
    {
        // Arrange
        var request = new CreateMovieRequest("", "Drama", 2020, 90);
        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("uk-UA");

        try
        {
            // Act
            var response = await PostMovieAsync(request);
            var problem = await ReadAsync<ValidationProblemDetails>(response);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            problem.Errors["title"].ShouldBe(["'Title' must not be empty."]);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public async Task GetMovies_SeededDatabase_ReturnsMoviesOrderedByTitle()
    {
        // Act
        var movies = await ReadAsync<List<MovieResponse>>(await Client.GetAsync("/api/movies"));

        // Assert
        movies.Count.ShouldBe(5);
        movies.Select(m => m.Title).ShouldBe(movies.Select(m => m.Title).OrderBy(t => t, StringComparer.Ordinal));
    }

    [Fact]
    public async Task CreateShowtime_ValidRequest_Returns201WithNamesAndEndTime()
    {
        // Act
        var showtime = await CreateShowtimeAsync(SeedData.InceptionId, TimeSpan.FromDays(1));

        // Assert
        showtime.MovieTitle.ShouldBe("Inception");
        showtime.AuditoriumName.ShouldBe("Hall 1");
        showtime.StartTime.ShouldBe(Now + TimeSpan.FromDays(1));
        showtime.EndTime.ShouldBe(showtime.StartTime + TimeSpan.FromMinutes(148));
    }

    [Fact]
    public async Task CreateShowtime_OverlappingInSameAuditorium_Returns409()
    {
        // Arrange
        var existing = await CreateShowtimeAsync(SeedData.InceptionId, TimeSpan.FromDays(1));

        // Act
        var response = await PostShowtimeAsync(SeedData.TheGodfatherId, SeedData.Hall1Id, existing.StartTime.AddMinutes(30));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateShowtime_StartsExactlyWhenPreviousEnds_Returns201()
    {
        // Arrange
        var existing = await CreateShowtimeAsync(SeedData.InceptionId, TimeSpan.FromDays(1));

        // Act
        var response = await PostShowtimeAsync(SeedData.TheGodfatherId, SeedData.Hall1Id, existing.EndTime);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateShowtime_StartTimeNotInFuture_Returns400()
    {
        // Act
        var response = await PostShowtimeAsync(SeedData.InceptionId, SeedData.Hall1Id, Now);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShowtime_UnknownMovie_Returns404()
    {
        // Act
        var response = await PostShowtimeAsync(Guid.CreateVersion7(), SeedData.Hall1Id, Now.AddDays(1));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
