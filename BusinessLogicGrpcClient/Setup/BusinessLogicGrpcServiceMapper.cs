using BusinessLogic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicGrpcClient.Setup;

public static class BusinessLogicGrpcServiceMapper
{
    /// Maps incoming requests to the specified TService type for BusinessLogic
    public static void MapBusinessLogicGrpcService(this WebApplication app)
    {
        app.MapGrpcService<BusinessLogicGrpcService>();
    }

    /// Maps incoming requests to the specified TService type for BusinessLogic
    public static void MapBusinessLogicGrpcService(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<BusinessLogicGrpcService>();
    }
}
