using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Infrastructure.Persistence;
using Cinema.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class ConcurrencyTests : IntegrationTestBase
{
    private const int Iterations = 20;

    [Fact]
    public async Task Reserve_TwoClientsSameSeatConcurrently_ExactlyOneWins()
    {
        for (var i = 0; i < Iterations; i++)
        {
            // Arrange
            var showtime = await CreateShowtimeAsync(startsIn: TimeSpan.FromDays(1 + i));
            var seatIds = await PickSeatsAsync(showtime.Id, 1);
            using var clientA = Factory.CreateClient();
            using var clientB = Factory.CreateClient();

            // Act
            var responses = await Task.WhenAll(
                PostReservationWithAsync(clientA, showtime.Id, seatIds),
                PostReservationWithAsync(clientB, showtime.Id, seatIds));
            var seats = await GetSeatsAsync(showtime.Id);

            // Assert
            responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1, $"iteration {i}: {Describe(responses)}");
            responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(1, $"iteration {i}: {Describe(responses)}");
            seats.Single(s => s.SeatId == seatIds[0]).Status.ShouldBe(SeatStatus.Reserved);
        }
    }

    [Fact]
    public async Task SaveChanges_TwoContextsReserveSameSeat_SecondThrowsConcurrencyException()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatId = (await PickSeatsAsync(showtime.Id, 1))[0];
        var now = Now.UtcDateTime;

        await using var first = CreateDbContext();
        await using var second = CreateDbContext();
        var seatInFirst = await first.ShowtimeSeats.SingleAsync(s => s.ShowtimeId == showtime.Id && s.SeatId == seatId);
        var seatInSecond = await second.ShowtimeSeats.SingleAsync(s => s.ShowtimeId == showtime.Id && s.SeatId == seatId);

        var firstReservation = Reservation.Create(showtime.Id, now);
        first.Reservations.Add(firstReservation);
        seatInFirst.Reserve(firstReservation.Id);
        await first.SaveChangesAsync();

        var secondReservation = Reservation.Create(showtime.Id, now);
        second.Reservations.Add(secondReservation);
        seatInSecond.Reserve(secondReservation.Id);

        // Act
        var act = () => second.SaveChangesAsync();

        // Assert
        await act.ShouldThrowAsync<DbUpdateConcurrencyException>();

        await using var verifier = CreateDbContext();
        var persisted = await verifier.ShowtimeSeats.AsNoTracking().SingleAsync(s => s.ShowtimeId == showtime.Id && s.SeatId == seatId);
        persisted.ReservationId.ShouldBe(firstReservation.Id);
        persisted.Version.ShouldBe(1);
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Factory.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static Task<HttpResponseMessage> PostReservationWithAsync(HttpClient client, Guid showtimeId, IReadOnlyList<Guid> seatIds) =>
        System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
            client, "/api/reservations", new Cinema.Application.Reservations.CreateReservationRequest(showtimeId, seatIds), Json);

    private static string Describe(HttpResponseMessage[] responses) =>
        string.Join(", ", responses.Select(r => (int)r.StatusCode));
}
