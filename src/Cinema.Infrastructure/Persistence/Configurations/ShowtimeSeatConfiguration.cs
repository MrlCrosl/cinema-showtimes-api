using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class ShowtimeSeatConfiguration : IEntityTypeConfiguration<ShowtimeSeat>
{
    public void Configure(EntityTypeBuilder<ShowtimeSeat> builder)
    {
        builder.ToTable("ShowtimeSeats");

        builder.HasKey(ss => new { ss.ShowtimeId, ss.SeatId });

        builder.Property(ss => ss.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(ss => ss.ReservationId).IsRequired(false);

        builder.Property(ss => ss.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasOne<Showtime>()
            .WithMany()
            .HasForeignKey(ss => ss.ShowtimeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Seat>()
            .WithMany()
            .HasForeignKey(ss => ss.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(ss => ss.ReservationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ss => ss.ReservationId);
    }
}
