using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ShowtimeId).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.HasOne<Showtime>()
            .WithMany()
            .HasForeignKey(r => r.ShowtimeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
