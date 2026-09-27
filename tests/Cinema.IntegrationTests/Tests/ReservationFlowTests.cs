using Cinema.Application.Reservations;
using Cinema.Domain.Enums;
using Cinema.Infrastructure.Persistence.Seed;
using Cinema.IntegrationTests.Infrastructure;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class ReservationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ReserveThenConfirm_FullFlow_SeatsEndUpSold()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync(SeedData.TheGodfatherId);
        var seatIds = await PickSeatsAsync(showtime.Id, 2);

        // Act
        var reserveResponse = await PostReservationAsync(showtime.Id, seatIds);
        var reservation = await ReadAsync<ReservationResponse>(reserveResponse);
        var confirmResponse = await ConfirmAsync(reservation.Reference);
        var confirmed = await ReadAsync<ReservationResponse>(confirmResponse);
        var seats = await GetSeatsAsync(showtime.Id);

        // Assert
        reserveResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        reserveResponse.Headers.Location!.ToString().ShouldEndWith($"/api/reservations/{reservation.Reference}");
        reservation.Reference.ShouldNotBe(Guid.Empty);
        reservation.State.ShouldBe(ReservationState.Pending);
        reservation.SeatsCount.ShouldBe(2);
        reservation.Seats.Select(s => s.SeatId).ShouldBe(seatIds);
        reservation.Movie.ShouldBe(new ReservationMovie(SeedData.TheGodfatherId, "The Godfather"));
        reservation.Auditorium.ShouldBe(new ReservationAuditorium(SeedData.Hall1Id, "Hall 1"));
        reservation.ExpiresAt.ShouldBe(reservation.CreatedAt + Cinema.Domain.Entities.Reservation.Lifetime);

        confirmResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        confirmed.State.ShouldBe(ReservationState.Confirmed);

        seats.Where(s => seatIds.Contains(s.SeatId)).ShouldAllBe(s => s.Status == SeatStatus.Sold);
        seats.Count(s => s.Status == SeatStatus.Free).ShouldBe(seats.Count - 2);
    }

    [Fact]
    public async Task Reserve_SeatHeldByActiveReservation_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatIds = await PickSeatsAsync(showtime.Id, 1);
        await ReserveAsync(showtime.Id, seatIds);

        // Act
        var response = await PostReservationAsync(showtime.Id, seatIds);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reserve_SoldSeat_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatIds = await PickSeatsAsync(showtime.Id, 1);
        var reservation = await ReserveAsync(showtime.Id, seatIds);
        (await ConfirmAsync(reservation.Reference)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await PostReservationAsync(showtime.Id, seatIds);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reserve_AfterShowtimeStarted_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync(startsIn: TimeSpan.FromHours(1));
        var seatIds = await PickSeatsAsync(showtime.Id, 1);
        Time.Advance(TimeSpan.FromHours(1));

        // Act
        var response = await PostReservationAsync(showtime.Id, seatIds);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reserve_SeatsFromTwoRows_ResponseOrdersSeatsByRowThenNumber()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seats = await GetSeatsAsync(showtime.Id);
        var lastSeat = seats[^1].SeatId;
        var firstSeat = seats[0].SeatId;

        // Act
        var reservation = await ReserveAsync(showtime.Id, [lastSeat, firstSeat]);

        // Assert
        reservation.Seats.Select(s => s.SeatId).ShouldBe([firstSeat, lastSeat]);
    }

    [Fact]
    public async Task GetSeats_NewShowtime_AllSeatsFreeOrderedByRowThenNumber()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();

        // Act
        var seats = await GetSeatsAsync(showtime.Id);

        // Assert
        seats.Count.ShouldBe(SeedData.Hall1Rows * SeedData.Hall1SeatsPerRow);
        seats.ShouldAllBe(s => s.Status == SeatStatus.Free);
        seats.Select(s => (s.Row, s.Number)).ShouldBe(seats.Select(s => (s.Row, s.Number)).OrderBy(x => x.Row).ThenBy(x => x.Number));
    }
}
