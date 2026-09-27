using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cinema.Infrastructure.Persistence.Conversions;

/// <summary>
/// Guards the UTC-only timestamp convention. On write, values must already be <see cref="DateTimeKind.Utc"/>;
/// any other kind throws <see cref="InvalidOperationException"/>. On read, SQLite drops the kind, so it is
/// re-applied as <see cref="DateTimeKind.Utc"/>.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    toProvider => EnsureUtc(toProvider),
    fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc))
{
    private static DateTime EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                $"Timestamps must be UTC before being persisted; got DateTimeKind.{value.Kind}.");
        }

        return value;
    }
}
