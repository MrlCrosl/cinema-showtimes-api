using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinema.Infrastructure.Persistence.Configurations;

internal sealed class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
{
    public void Configure(EntityTypeBuilder<Showtime> builder)
    {
        builder.ToTable("Showtimes");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.MovieId).IsRequired();
        builder.Property(s => s.AuditoriumId).IsRequired();
        builder.Property(s => s.StartTime).IsRequired();

        builder.HasOne<Movie>()
            .WithMany()
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Auditorium>()
            .WithMany()
            .HasForeignKey(s => s.AuditoriumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.AuditoriumId, s.StartTime });
    }
}
