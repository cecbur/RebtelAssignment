using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers server-side BusinessLogic services for dependency injection.
    /// Use this in the composition root when hosting BusinessLogic as a gRPC service.
    /// </summary>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        services.AddScoped<PatronActivity>();
        services.AddScoped<BorrowingPatterns>();
        services.AddScoped<BookPatterns>();
        services.AddScoped<Facade>();

        return services;
    }
}
