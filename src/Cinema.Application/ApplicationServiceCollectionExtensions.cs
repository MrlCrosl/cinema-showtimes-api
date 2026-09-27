using System.Text;
using System.Text.Json;
using Cinema.Application.Movies;
using Cinema.Application.Reservations;
using Cinema.Application.Showtimes;
using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Error keys must match the JSON contract (title, year, durationMinutes), not the C# property names.
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

        // Messages keep the human-readable form ("Duration Minutes"), which would otherwise follow the camelCase key.
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) =>
            member is null ? null : SplitPascalCase(member.Name);

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);

        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IShowtimeService, ShowtimeService>();
        services.AddScoped<IReservationService, ReservationService>();

        return services;
    }

    private static string SplitPascalCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);

        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(name[i]);
        }

        return builder.ToString();
    }
}
