using Cinema.Application.Abstractions;
using Cinema.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.Infrastructure;

public static class DependencyInjection
{
    /// <param name="connectionString">
    /// Resolved when DbContext options are built (per scope), not at registration,
    /// so hosts that finalize configuration late (e.g. WebApplicationFactory) are honoured.
    /// </param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Func<IServiceProvider, string> connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<AppDbContext>((serviceProvider, options) => options.UseSqlite(connectionString(serviceProvider)));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
