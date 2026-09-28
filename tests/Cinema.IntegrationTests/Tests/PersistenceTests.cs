using System.Net.Http.Json;
using Cinema.Application.Movies;
using Cinema.IntegrationTests.Infrastructure;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

/// <summary>US-5: data written by one app instance is read back by a new instance on the same SQLite file.</summary>
public sealed class PersistenceTests : IDisposable
{
    private readonly string _databasePath = CinemaApiFactory.NewDatabasePath();

    [Fact]
    public async Task GetMovie_AfterRestartOnSameDatabase_Returns200WithSameMovie()
    {
        // Arrange
        MovieResponse created;
        await using (var firstRun = new CinemaApiFactory(_databasePath))
        {
            using var client = firstRun.CreateClient();
            var createResponse = await client.PostAsJsonAsync("/api/movies", new CreateMovieRequest("Heat", "Crime", 1995, 170));
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            created = (await createResponse.Content.ReadFromJsonAsync<MovieResponse>())!;
        }

        await using var secondRun = new CinemaApiFactory(_databasePath);
        using var secondClient = secondRun.CreateClient();

        // Act
        var response = await secondClient.GetAsync($"/api/movies/{created.Id}");
        var fetched = await response.Content.ReadFromJsonAsync<MovieResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        fetched.ShouldBe(created);
    }

    public void Dispose()
    {
        CinemaApiFactory.DeleteDatabase(_databasePath);
    }
}
