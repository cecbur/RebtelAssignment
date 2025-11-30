using DataStorage.Services;
using Microsoft.AspNetCore.Builder;

namespace DataStorageGrpcClient.Setup;

public static class DataStorageGrpcServiceMapper
{
    /// <summary>
    /// Maps incoming requests to DataStorage gRPC services
    /// </summary>
    public static void MapDataStorageGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<LoanGrpcService>();
        app.MapGrpcService<BorrowingPatternGrpcService>();
        app.MapGrpcService<BookGrpcService>();
    }
}
