using Cinema.Domain.Entities;
using Cinema.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Category).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Year).IsRequired();
        builder.Property(m => m.DurationMinutes).IsRequired();

        builder.HasData(SeedData.Movies);
    }
}
