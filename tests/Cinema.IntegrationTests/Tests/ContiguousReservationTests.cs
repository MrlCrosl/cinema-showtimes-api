using System.Net.Http.Json;
using Cinema.Application.Reservations;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Infrastructure.Persistence.Seed;
using Cinema.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class ContiguousReservationTests : IntegrationTestBase
{
    [Fact]
    public async Task ReserveContiguous_FreshShowtime_ReservesThreeAdjacentSeatsInOneRow()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();

        // Act
        var response = await PostContiguousReservationAsync(showtime.Id, 3);
        var reservation = await ReadAsync<ReservationResponse>(response);
        var seats = await GetSeatsAsync(showtime.Id);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.ToString().ShouldEndWith($"/api/reservations/{reservation.Reference}");
        reservation.SeatsCount.ShouldBe(3);
        reservation.Seats.Select(s => s.Row).Distinct().Count().ShouldBe(1);
        reservation.Seats.Select(s => s.Number).ShouldBe([1, 2, 3]);
        seats.Where(s => reservation.Seats.Select(r => r.SeatId).Contains(s.SeatId)).ShouldAllBe(s => s.Status == SeatStatus.Reserved);
        seats.Count(s => s.Status == SeatStatus.Reserved).ShouldBe(3);
    }

    [Fact]
    public async Task ReserveContiguous_FirstRowHasOnlyNonAdjacentSeats_BlockIsNotInFirstRow()
    {
        // Arrange: row 1 keeps only seats 3 and 6 free
        var showtime = await CreateShowtimeAsync();
        var row1Taken = Enumerable.Range(1, SeedData.Hall1SeatsPerRow)
            .Where(n => n is not (3 or 6))
            .Select(n => (Row: 1, Number: n))
            .ToArray();
        await ReserveAsync(showtime.Id, await SeatIdsAtAsync(showtime.Id, row1Taken));

        // Act
        var reservation = await ReserveContiguousAsync(showtime.Id, 2);

        // Assert
        reservation.Seats.ShouldAllBe(s => s.Row == 2);
        reservation.Seats.Select(s => s.Number).ShouldBe([1, 2]);
    }

    [Fact]
    public async Task ReserveContiguous_EverySecondSeatTaken_Returns409()
    {
        // Arrange: odd seats of every row are held (two reservations, as one may hold at most 10 seats)
        var showtime = await CreateShowtimeAsync();
        var oddSeats = Enumerable.Range(1, SeedData.Hall1Rows)
            .SelectMany(row => Enumerable.Range(1, SeedData.Hall1SeatsPerRow).Where(n => n % 2 == 1).Select(n => (Row: row, Number: n)))
            .ToArray();
        var oddSeatIds = await SeatIdsAtAsync(showtime.Id, oddSeats);
        foreach (var chunk in oddSeatIds.Chunk(CreateReservationRequestValidator.MaxSeatsPerReservation))
        {
            await ReserveAsync(showtime.Id, chunk);
        }

        // Act
        var response = await PostContiguousReservationAsync(showtime.Id, 2);
        var problem = await ReadAsync<ProblemDetails>(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Detail.ShouldBe("No contiguous block of 2 seats is available.");
    }

    [Fact]
    public async Task ReserveContiguous_HoldOnFirstSeatsExpired_ReusesThoseSeats()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var firstThree = await SeatIdsAtAsync(showtime.Id, (1, 1), (1, 2), (1, 3));
        await ReserveAsync(showtime.Id, firstThree);
        Time.Advance(Reservation.Lifetime);

        // Act
        var reservation = await ReserveContiguousAsync(showtime.Id, 3);

        // Assert
        reservation.Seats.Select(s => s.SeatId).ShouldBe(firstThree);
        reservation.Seats.Select(s => (s.Row, s.Number)).ShouldBe([(1, 1), (1, 2), (1, 3)]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(CreateReservationRequestValidator.MaxSeatsPerReservation + 1)]
    public async Task ReserveContiguous_CountOutOfRange_Returns400WithCamelCaseCountError(int count)
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();

        // Act
        var response = await PostContiguousReservationAsync(showtime.Id, count);
        var problem = await ReadAsync<ValidationProblemDetails>(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Keys.ShouldBe(["count"]);
    }

    [Fact]
    public async Task ReserveContiguous_UnknownShowtime_Returns404()
    {
        // Act
        var response = await PostContiguousReservationAsync(Guid.CreateVersion7(), 2);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReserveContiguous_ShowtimeStarted_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync(startsIn: TimeSpan.FromHours(1));
        Time.Advance(TimeSpan.FromHours(1));

        // Act
        var response = await PostContiguousReservationAsync(showtime.Id, 2);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReserveContiguous_TwoClientsConcurrently_NeverOverlap()
    {
        const int iterations = 10;
        const int count = 4;

        for (var i = 0; i < iterations; i++)
        {
            // Arrange
            var showtime = await CreateShowtimeAsync(startsIn: TimeSpan.FromDays(1 + i));
            using var clientA = Factory.CreateClient();
            using var clientB = Factory.CreateClient();

            // Act
            var responses = await Task.WhenAll(
                PostContiguousWithAsync(clientA, showtime.Id, count),
                PostContiguousWithAsync(clientB, showtime.Id, count));
            var created = new List<ReservationResponse>();
            foreach (var response in responses.Where(r => r.StatusCode == HttpStatusCode.Created))
            {
                created.Add(await ReadAsync<ReservationResponse>(response));
            }
            var seats = await GetSeatsAsync(showtime.Id);

            // Assert
            var statuses = responses.Select(r => r.StatusCode).ToList();
            statuses.ShouldAllBe(s => s == HttpStatusCode.Created || s == HttpStatusCode.Conflict, $"iteration {i}: {string.Join(", ", statuses)}");
            created.Count.ShouldBeInRange(1, 2, $"iteration {i}");
            created.SelectMany(r => r.Seats.Select(s => s.SeatId)).ShouldBeUnique($"iteration {i}");
            created.ShouldAllBe(r => r.SeatsCount == count);
            seats.Count(s => s.Status == SeatStatus.Reserved).ShouldBe(count * created.Count, $"iteration {i}");
        }
    }

    private static Task<HttpResponseMessage> PostContiguousWithAsync(HttpClient client, Guid showtimeId, int count) =>
        client.PostAsJsonAsync("/api/reservations/contiguous", new CreateContiguousReservationRequest(showtimeId, count), Json);
}
