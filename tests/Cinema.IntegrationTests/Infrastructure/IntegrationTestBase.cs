using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Application.Movies;
using Cinema.Application.Reservations;
using Cinema.Application.Showtimes;
using Cinema.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Cinema.IntegrationTests.Infrastructure;

/// <summary>
/// One factory, database and HttpClient per test (xUnit creates a new class instance per test).
/// Helpers keep test bodies to Arrange/Act/Assert; they assert the setup calls succeed so failures point at the right step.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected CinemaApiFactory Factory { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected FakeTimeProvider Time => Factory.Time;

    protected DateTimeOffset Now => Time.GetUtcNow();

    public Task InitializeAsync()
    {
        Factory = new CinemaApiFactory();
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected async Task<ShowtimeResponse> CreateShowtimeAsync(Guid? movieId = null, TimeSpan? startsIn = null)
    {
        var request = new CreateShowtimeRequest(
            movieId ?? SeedData.InceptionId,
            SeedData.Hall1Id,
            Now + (startsIn ?? TimeSpan.FromDays(1)));

        var response = await Client.PostAsJsonAsync("/api/showtimes", request, Json);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ShowtimeResponse>(Json))!;
    }

    protected Task<HttpResponseMessage> PostShowtimeAsync(Guid movieId, Guid auditoriumId, DateTimeOffset startTime) =>
        Client.PostAsJsonAsync("/api/showtimes", new CreateShowtimeRequest(movieId, auditoriumId, startTime), Json);

    protected async Task<IReadOnlyList<SeatAvailabilityResponse>> GetSeatsAsync(Guid showtimeId)
    {
        var response = await Client.GetAsync($"/api/showtimes/{showtimeId}/seats");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<SeatAvailabilityResponse>>(Json))!;
    }

    /// <summary>The first <paramref name="count"/> seat ids in row/number order.</summary>
    protected async Task<List<Guid>> PickSeatsAsync(Guid showtimeId, int count, int skip = 0)
    {
        var seats = await GetSeatsAsync(showtimeId);
        return seats.Skip(skip).Take(count).Select(s => s.SeatId).ToList();
    }

    protected Task<HttpResponseMessage> PostReservationAsync(Guid showtimeId, IReadOnlyList<Guid> seatIds) =>
        Client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(showtimeId, seatIds), Json);

    protected async Task<ReservationResponse> ReserveAsync(Guid showtimeId, IReadOnlyList<Guid> seatIds)
    {
        var response = await PostReservationAsync(showtimeId, seatIds);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ReservationResponse>(Json))!;
    }

    protected Task<HttpResponseMessage> ConfirmAsync(Guid reference) =>
        Client.PostAsync($"/api/reservations/{reference}/confirm", content: null);

    protected Task<HttpResponseMessage> GetReservationAsync(Guid reference) =>
        Client.GetAsync($"/api/reservations/{reference}");

    protected Task<HttpResponseMessage> PostMovieAsync(CreateMovieRequest request) =>
        Client.PostAsJsonAsync("/api/movies", request, Json);

    protected static Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<T>(Json)!;
}
