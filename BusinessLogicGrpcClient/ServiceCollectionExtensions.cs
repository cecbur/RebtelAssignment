using BusinessLogicContracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicGrpcClient;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BusinessLogic gRPC client for dependency injection.
    /// Use this in consumers that need to call BusinessLogic via gRPC.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="grpcServerAddress">The gRPC server address (e.g., "http://localhost:5001")</param>
    public static IServiceCollection AddBusinessLogicGrpcClient(this IServiceCollection services, string grpcServerAddress)
    {
        services.AddScoped<IBusinessLogicFacade>(sp =>
            new BusinessLogicGrpcFacade(grpcServerAddress));

        return services;
    }
}
