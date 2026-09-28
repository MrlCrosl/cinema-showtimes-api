using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Cinema.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the API in-process against a private SQLite file database and a controllable clock.
/// The production startup path (migrations, seed) runs unchanged; only configuration and TimeProvider differ.
/// Pass a <c>databasePath</c> to reuse an existing file (for example across a simulated restart); the caller then owns
/// the file and deletes it with <see cref="DeleteDatabase"/>.
/// </summary>
public sealed class CinemaApiFactory(string? databasePath = null) : WebApplicationFactory<Program>
{
    /// <summary>Fixed, deterministic "now" for every test. Showtimes are created relative to it.</summary>
    public static readonly DateTimeOffset StartInstant = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly bool _ownsDatabase = databasePath is null;

    public FakeTimeProvider Time { get; } = new(StartInstant);

    public string DatabasePath { get; } = databasePath ?? NewDatabasePath();

    public string ConnectionString => $"Data Source={DatabasePath}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Cinema"] = ConnectionString
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        SqliteConnection.ClearAllPools();

        if (_ownsDatabase)
        {
            DeleteDatabase(DatabasePath);
        }
    }

    public static string NewDatabasePath() => Path.Combine(Path.GetTempPath(), $"cinema-tests-{Guid.NewGuid():N}.db");

    public static void DeleteDatabase(string path)
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            File.Delete(path + suffix);
        }
    }
}
