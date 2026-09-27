using Cinema.Domain.Entities;
using Cinema.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class AuditoriumConfiguration : IEntityTypeConfiguration<Auditorium>
{
    public void Configure(EntityTypeBuilder<Auditorium> builder)
    {
        builder.ToTable("Auditoriums");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(a => a.Seats)
            .WithOne()
            .HasForeignKey(s => s.AuditoriumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(a => a.Seats)
            .HasField("_seats")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.Auditoriums);
    }
}
