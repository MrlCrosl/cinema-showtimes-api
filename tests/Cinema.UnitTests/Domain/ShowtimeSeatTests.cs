using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Shouldly;

namespace Cinema.UnitTests.Domain;

public sealed class ShowtimeSeatTests
{
    private static readonly DateTime Now = new(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static readonly Guid ShowtimeId = Guid.CreateVersion7();

    private static readonly Guid SeatId = Guid.CreateVersion7();

    [Fact]
    public void Create_ValidInput_IsFreeWithVersionZero()
    {
        // Act
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);

        // Assert
        seat.Status.ShouldBe(SeatStatus.Free);
        seat.ReservationId.ShouldBeNull();
        seat.Version.ShouldBe(0);
    }

    [Fact]
    public void IsAvailable_FreeSeat_ReturnsTrue()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);

        // Act
        var available = seat.IsAvailable(null, Now);

        // Assert
        available.ShouldBeTrue();
    }

    [Fact]
    public void IsAvailable_SoldSeat_ReturnsFalse()
    {
        // Arrange
        var holder = Reservation.Create(ShowtimeId, Now);
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(holder.Id);
        seat.Sell();

        // Act
        var available = seat.IsAvailable(holder, Now.AddDays(1));

        // Assert
        available.ShouldBeFalse();
    }

    [Fact]
    public void IsAvailable_ReservedWithActiveHolder_ReturnsFalse()
    {
        // Arrange
        var holder = Reservation.Create(ShowtimeId, Now);
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(holder.Id);

        // Act
        var available = seat.IsAvailable(holder, holder.ExpiresAt.AddTicks(-1));

        // Assert
        available.ShouldBeFalse();
    }

    [Fact]
    public void IsAvailable_ReservedWithExpiredHolder_ReturnsTrue()
    {
        // Arrange
        var holder = Reservation.Create(ShowtimeId, Now);
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(holder.Id);

        // Act
        var available = seat.IsAvailable(holder, holder.ExpiresAt);

        // Assert
        available.ShouldBeTrue();
    }

    [Fact]
    public void IsAvailable_ReservedWithNullHolder_ReturnsFalse()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(Guid.CreateVersion7());

        // Act
        var available = seat.IsAvailable(null, Now.AddDays(1));

        // Assert
        available.ShouldBeFalse();
    }

    [Fact]
    public void Reserve_FreeSeat_SetsReservedWithReservationAndBumpsVersion()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        var reservationId = Guid.CreateVersion7();

        // Act
        seat.Reserve(reservationId);

        // Assert
        seat.Status.ShouldBe(SeatStatus.Reserved);
        seat.ReservationId.ShouldBe(reservationId);
        seat.Version.ShouldBe(1);
    }

    [Fact]
    public void Reserve_ReservedSeat_ReplacesReservationAndBumpsVersion()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(Guid.CreateVersion7());
        var newReservationId = Guid.CreateVersion7();

        // Act
        seat.Reserve(newReservationId);

        // Assert
        seat.ReservationId.ShouldBe(newReservationId);
        seat.Version.ShouldBe(2);
    }

    [Fact]
    public void Reserve_SoldSeat_ThrowsInvalidOperationException()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(Guid.CreateVersion7());
        seat.Sell();

        // Act
        var act = () => seat.Reserve(Guid.CreateVersion7());

        // Assert
        act.ShouldThrow<InvalidOperationException>();
        seat.Status.ShouldBe(SeatStatus.Sold);
    }

    [Fact]
    public void Reserve_EmptyReservationId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);

        // Act
        var act = () => seat.Reserve(Guid.Empty);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Sell_ReservedSeat_SetsSoldAndBumpsVersion()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        var reservationId = Guid.CreateVersion7();
        seat.Reserve(reservationId);

        // Act
        seat.Sell();

        // Assert
        seat.Status.ShouldBe(SeatStatus.Sold);
        seat.ReservationId.ShouldBe(reservationId);
        seat.Version.ShouldBe(2);
    }

    [Fact]
    public void Sell_FreeSeat_ThrowsInvalidOperationException()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);

        // Act
        var act = () => seat.Sell();

        // Assert
        act.ShouldThrow<InvalidOperationException>();
        seat.Version.ShouldBe(0);
    }

    [Fact]
    public void Sell_SoldSeat_ThrowsInvalidOperationException()
    {
        // Arrange
        var seat = ShowtimeSeat.Create(ShowtimeId, SeatId);
        seat.Reserve(Guid.CreateVersion7());
        seat.Sell();

        // Act
        var act = () => seat.Sell();

        // Assert
        act.ShouldThrow<InvalidOperationException>();
        seat.Version.ShouldBe(2);
    }
}
