using BusinessLogicGrpcClient.Setup;
using DataStorageContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

/// <summary>
/// Base class for tests that require a gRPC test server with mocked DataStorage layer.
/// Provides infrastructure for setting up an in-memory gRPC server for integration testing.
/// </summary>
public abstract class DataStorageMockGrpcTestFixtureBase
{
    private IHost? _testServer;
    protected string ServerAddress { get; private set; } = null!;
    protected Mock<ILoanRepository> MockLoanRepository { get; private set; } = null!;
    private Mock<IBorrowingPatternRepository> MockBorrowingPatternRepository { get; set; } = null!;
    private Mock<ILogger<BusinessLogic.Services.BusinessLogicGrpcService>> _mockGrpcServiceLogger = null!;

    protected async Task SetUpGrpcServer()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        MockBorrowingPatternRepository = new Mock<IBorrowingPatternRepository>();
        _mockGrpcServiceLogger = new Mock<ILogger<BusinessLogic.Services.BusinessLogicGrpcService>>();

        var port = FindFreePortForTestServer();
        ServerAddress = $"http://localhost:{port}";
        _testServer = await CreateAndStartTestGrpcServer(ServerAddress, port, MockLoanRepository.Object, MockBorrowingPatternRepository.Object, _mockGrpcServiceLogger.Object);
    }

    protected async Task TearDownGrpcServer()
    {
        if (_testServer != null)
        {
            await _testServer.StopAsync();
            _testServer.Dispose();
        }
    }

    private static async Task<IHost> CreateAndStartTestGrpcServer(string serverAddress, int port,
        ILoanRepository mockLoanRepository,
        IBorrowingPatternRepository mockBorrowingPatternRepository,
        ILogger<BusinessLogic.Services.BusinessLogicGrpcService> mockGrpcServiceLogger)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls(serverAddress);
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(port, o => o.Protocols = HttpProtocols.Http2);
                });
                webBuilder.ConfigureServices(services =>
                {
                    // Register mocked repositories
                    services.AddSingleton(mockLoanRepository);
                    services.AddSingleton(mockBorrowingPatternRepository);
                    services.AddSingleton(mockGrpcServiceLogger);

                    services.AddBusinessLogicGrpcClient(serverAddress);

                    services.AddGrpc();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapBusinessLogicGrpcService();
                    });
                });
            });

        var testServer = hostBuilder.Build();
        await testServer.StartAsync();
        return testServer;
    }

    private static int FindFreePortForTestServer()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}