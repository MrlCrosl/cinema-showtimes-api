using Cinema.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class ReservationErrorTests : IntegrationTestBase
{
    private static readonly Guid UnknownId = new("00000000-0000-0000-0000-00000000dead");

    [Fact]
    public async Task Reserve_DuplicateSeatIds_Returns400WithCamelCaseSeatIdsError()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatIds = await PickSeatsAsync(showtime.Id, 1);

        // Act
        var response = await PostReservationAsync(showtime.Id, [seatIds[0], seatIds[0]]);
        var problem = await ReadAsync<ValidationProblemDetails>(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Keys.ShouldBe(["seatIds"]);
        problem.Errors["seatIds"].ShouldContain(m => m.Contains("duplicates", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Reserve_UnknownSeatId_Returns400()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();

        // Act
        var response = await PostReservationAsync(showtime.Id, [UnknownId]);
        var problem = await ReadAsync<ProblemDetails>(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Detail.ShouldNotBeNull().ShouldContain(UnknownId.ToString());
    }

    [Fact]
    public async Task Reserve_UnknownShowtime_Returns404()
    {
        // Act
        var response = await PostReservationAsync(UnknownId, [Guid.CreateVersion7()]);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReservation_UnknownReference_Returns404()
    {
        // Act
        var response = await GetReservationAsync(UnknownId);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Confirm_UnknownReference_Returns404()
    {
        // Act
        var response = await ConfirmAsync(UnknownId);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSeats_UnknownShowtime_Returns404()
    {
        // Act
        var response = await Client.GetAsync($"/api/showtimes/{UnknownId}/seats");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ErrorResponses_AreProblemDetailsWithTraceId()
    {
        // Act
        var response = await GetReservationAsync(UnknownId);
        var problem = await ReadAsync<ProblemDetails>(response);

        // Assert
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        problem.Status.ShouldBe(404);
        problem.Extensions.Keys.ShouldContain("traceId");
    }
}
