using Cinema.Domain.Entities;
using Cinema.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.AuditoriumId).IsRequired();
        builder.Property(s => s.Row).IsRequired();
        builder.Property(s => s.Number).IsRequired();

        builder.HasIndex(s => new { s.AuditoriumId, s.Row, s.Number }).IsUnique();

        builder.HasData(SeedData.Seats);
    }
}
