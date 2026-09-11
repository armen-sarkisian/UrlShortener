using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.Services;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
        services.AddScoped<ICodeSequence, SqlServerCodeSequence>();
        services.AddScoped<IShortUrlService, ShortUrlService>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
