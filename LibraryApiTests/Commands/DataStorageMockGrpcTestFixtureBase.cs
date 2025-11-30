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

        _testServer = new TestServerBuilder()
            .WithMockedRepositories(
                MockLoanRepository.Object,
                MockBorrowingPatternRepository.Object,
                _mockGrpcServiceLogger.Object)
            .OnPort(port)
            .Build();

        await _testServer.StartAsync();
    }

    protected async Task TearDownGrpcServer()
    {
        if (_testServer != null)
        {
            await _testServer.StopAsync();
            _testServer.Dispose();
        }
    }

    private static int FindFreePortForTestServer()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private class TestServerBuilder
    {
        private ILoanRepository? _loanRepository;
        private IBorrowingPatternRepository? _borrowingPatternRepository;
        private ILogger<BusinessLogic.Services.BusinessLogicGrpcService>? _grpcServiceLogger;
        private int _port;
        private string _serverAddress = null!;

        public TestServerBuilder WithMockedRepositories(
            ILoanRepository loanRepo,
            IBorrowingPatternRepository borrowingRepo,
            ILogger<BusinessLogic.Services.BusinessLogicGrpcService> grpcServiceLogger)
        {
            _loanRepository = loanRepo;
            _borrowingPatternRepository = borrowingRepo;
            _grpcServiceLogger = grpcServiceLogger;
            return this;
        }

        public TestServerBuilder OnPort(int port)
        {
            _port = port;
            _serverAddress = $"http://localhost:{port}";
            return this;
        }

        public IHost Build()
        {
            if (_loanRepository == null || _borrowingPatternRepository == null || _grpcServiceLogger == null)
            {
                throw new InvalidOperationException("Mocked repositories must be configured before building the server.");
            }

            if (_port == 0)
            {
                throw new InvalidOperationException("Port must be configured before building the server.");
            }

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(_serverAddress);
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(_port, o => o.Protocols = HttpProtocols.Http2);
                    });
                    webBuilder.ConfigureServices(services =>
                    {
                        // Register mocked repositories
                        services.AddSingleton(_loanRepository);
                        services.AddSingleton(_borrowingPatternRepository);
                        services.AddSingleton(_grpcServiceLogger);

                        services.AddBusinessLogicGrpcClient(_serverAddress);

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

            return hostBuilder.Build();
        }
    }
}