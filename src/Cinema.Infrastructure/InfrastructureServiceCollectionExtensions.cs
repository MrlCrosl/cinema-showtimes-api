using Cinema.Application.Abstractions;
using Cinema.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <param name="services">The service collection to register infrastructure services in.</param>
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
