using Cinema.Application.Reservations;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.IntegrationTests.Infrastructure;
using Shouldly;

namespace Cinema.IntegrationTests.Tests;

public sealed class ReservationExpiryTests : IntegrationTestBase
{
    [Fact]
    public async Task Expiry_AfterLifetime_SeatsFreeAgainAndReservationReportedExpired()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatIds = await PickSeatsAsync(showtime.Id, 2);
        var first = await ReserveAsync(showtime.Id, seatIds);
        Time.Advance(Reservation.Lifetime);

        // Act
        var seatsAfterExpiry = await GetSeatsAsync(showtime.Id);
        var secondResponse = await PostReservationAsync(showtime.Id, seatIds);
        var firstResponse = await GetReservationAsync(first.Reference);
        var firstAfterExpiry = await ReadAsync<ReservationResponse>(firstResponse);
        var confirmResponse = await ConfirmAsync(first.Reference);

        // Assert
        seatsAfterExpiry.Where(s => seatIds.Contains(s.SeatId)).ShouldAllBe(s => s.Status == SeatStatus.Free);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstAfterExpiry.State.ShouldBe(ReservationState.Expired);
        confirmResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Expiry_SeatsTakenOverAfterExpiry_OldReservationNoLongerListsThem()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var seatIds = await PickSeatsAsync(showtime.Id, 1);
        var first = await ReserveAsync(showtime.Id, seatIds);
        Time.Advance(Reservation.Lifetime);
        var second = await ReserveAsync(showtime.Id, seatIds);

        // Act
        var seats = await GetSeatsAsync(showtime.Id);
        var firstAfterTakeover = await ReadAsync<ReservationResponse>(await GetReservationAsync(first.Reference));

        // Assert
        seats.Single(s => s.SeatId == seatIds[0]).Status.ShouldBe(SeatStatus.Reserved);
        second.State.ShouldBe(ReservationState.Pending);
        firstAfterTakeover.SeatsCount.ShouldBe(0);
    }

    [Fact]
    public async Task Confirm_OneSecondBeforeLifetimeEnds_Returns200()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var reservation = await ReserveAsync(showtime.Id, await PickSeatsAsync(showtime.Id, 1));
        Time.Advance(Reservation.Lifetime - TimeSpan.FromSeconds(1));

        // Act
        var response = await ConfirmAsync(reservation.Reference);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync<ReservationResponse>(response)).State.ShouldBe(ReservationState.Confirmed);
    }

    [Fact]
    public async Task Confirm_ExactlyAtLifetimeEnd_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var reservation = await ReserveAsync(showtime.Id, await PickSeatsAsync(showtime.Id, 1));
        Time.Advance(Reservation.Lifetime);

        // Act
        var response = await ConfirmAsync(reservation.Reference);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadAsync<ReservationResponse>(await GetReservationAsync(reservation.Reference))).State.ShouldBe(ReservationState.Expired);
    }

    [Fact]
    public async Task Confirm_AlreadyConfirmed_Returns409()
    {
        // Arrange
        var showtime = await CreateShowtimeAsync();
        var reservation = await ReserveAsync(showtime.Id, await PickSeatsAsync(showtime.Id, 1));
        (await ConfirmAsync(reservation.Reference)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await ConfirmAsync(reservation.Reference);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}
