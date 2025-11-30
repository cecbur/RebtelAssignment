using DataStorage;
using DataStorageContracts;
using Microsoft.Extensions.DependencyInjection;

namespace DataStorageGrpcClient.Setup;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers DataStorage gRPC client for dependency injection.
    /// Use this in consumers that need to call DataStorage via gRPC.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="grpcServerAddress">The gRPC server address (e.g., "http://localhost:5001")</param>
    public static IServiceCollection AddDataStorageGrpcClient(this IServiceCollection services, string grpcServerAddress)
    {
        services.AddDataStorageServices();

        services.AddScoped<ILoanRepository>(sp =>
            new LoanRepository(grpcServerAddress));
        services.AddScoped<IBorrowingPatternRepository>(sp =>
            new BorrowingPatternRepository(grpcServerAddress));
        services.AddScoped<IBookRepository>(sp =>
            new BookRepository(grpcServerAddress));

        return services;
    }
}
