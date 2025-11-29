using BusinessLogic.Services;
using Microsoft.AspNetCore.Builder;

namespace BusinessLogicGrpcClient.Setup;

public static class BusinessLogicGrpcServiceMapper
{
    /// Maps incoming requests to the specified TService type for BusinessLogic
    public static void MapBusinessLogicGrpcService(this WebApplication app)
    {
        app.MapGrpcService<BusinessLogicGrpcService>();
    }
}
