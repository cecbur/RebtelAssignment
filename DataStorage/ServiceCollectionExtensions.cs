using Microsoft.Extensions.DependencyInjection;

namespace DataStorage;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers server-side DataStorage repositories for dependency injection.
    /// Use this in the composition root when hosting DataStorage as a gRPC service.
    /// Note: AuthorRepository and PatronRepository are only used internally by DataStorage.
    /// </summary>
    public static IServiceCollection AddDataStorageServices(this IServiceCollection services)
    {
        services.AddScoped<Repositories.LoanRepository>();
        services.AddScoped<RepositoriesMultipleTables.BorrowingPatternRepository>();
        services.AddScoped<Repositories.BookRepository>();

        return services;
    }
}
