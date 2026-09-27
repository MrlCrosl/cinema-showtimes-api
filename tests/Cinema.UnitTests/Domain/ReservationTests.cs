using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Exceptions;
using Shouldly;

namespace Cinema.UnitTests.Domain;

public sealed class ReservationTests
{
    private static readonly DateTime Now = new(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static readonly Guid ShowtimeId = Guid.CreateVersion7();

    [Fact]
    public void Create_ValidInput_SetsPendingWithExpiryAfterLifetime()
    {
        // Act
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Assert
        reservation.Id.ShouldNotBe(Guid.Empty);
        reservation.ShowtimeId.ShouldBe(ShowtimeId);
        reservation.CreatedAt.ShouldBe(Now);
        reservation.ExpiresAt.ShouldBe(Now + Reservation.Lifetime);
        reservation.Status.ShouldBe(ReservationStatus.Pending);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Create_NonUtcTime_ThrowsArgumentException(DateTimeKind kind)
    {
        // Arrange
        var nonUtc = DateTime.SpecifyKind(Now, kind);

        // Act
        var act = () => Reservation.Create(ShowtimeId, nonUtc);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyShowtimeId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => Reservation.Create(Guid.Empty, Now);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsExpired_OneTickBeforeExpiry_ReturnsFalse()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Act
        var expired = reservation.IsExpired(reservation.ExpiresAt.AddTicks(-1));

        // Assert
        expired.ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_ExactlyAtExpiry_ReturnsTrue()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Act
        var expired = reservation.IsExpired(reservation.ExpiresAt);

        // Assert
        expired.ShouldBeTrue();
    }

    [Fact]
    public void IsExpired_ConfirmedReservationLongAfterExpiry_ReturnsFalse()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);
        reservation.Confirm(Now);

        // Act
        var expired = reservation.IsExpired(reservation.ExpiresAt.AddDays(1));

        // Assert
        expired.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_WhilePending_SetsConfirmed()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Act
        reservation.Confirm(reservation.ExpiresAt.AddTicks(-1));

        // Assert
        reservation.Status.ShouldBe(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ThrowsConflictException()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);
        reservation.Confirm(Now);

        // Act
        var act = () => reservation.Confirm(Now);

        // Assert
        act.ShouldThrow<ConflictException>();
    }

    [Fact]
    public void Confirm_ExactlyAtExpiry_ThrowsConflictException()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Act
        var act = () => reservation.Confirm(reservation.ExpiresAt);

        // Assert
        act.ShouldThrow<ConflictException>();
        reservation.Status.ShouldBe(ReservationStatus.Pending);
    }

    [Fact]
    public void Confirm_NonUtcTime_ThrowsArgumentException()
    {
        // Arrange
        var reservation = Reservation.Create(ShowtimeId, Now);

        // Act
        var act = () => reservation.Confirm(DateTime.SpecifyKind(Now, DateTimeKind.Unspecified));

        // Assert
        act.ShouldThrow<ArgumentException>();
    }
}
