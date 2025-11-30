using BusinessLogicContracts.Interfaces;
using BusinessLogicGrpcClient;
using BusinessLogicGrpcClient.Setup;
using BusinessModels;
using DataStorageContracts;
using LibraryApi.Commands.AssignmentCommands;
using LibraryApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

[TestFixture]
public class GetBooksSortedByMostLoanedCommandTests
{
    private Mock<ILoanRepository> _mockLoanRepository = null!;
    private Mock<IBorrowingPatternRepository> _mockBorrowingPatternRepository = null!;
    private Mock<ILogger<AssignmentController>> _mockCommandLogger = null!;
    private Mock<ILogger<BusinessLogic.Services.BusinessLogicGrpcService>> _mockGrpcServiceLogger = null!;
    private GetBooksSortedByMostLoanedCommand _sut = null!;
    private IHost? _testServer;
    private string _serverAddress = null!;
    private IBusinessLogicFacade _businessLogicFacade = null!;

    [SetUp]
    public async Task SetUp()
    {
        _mockLoanRepository = new Mock<ILoanRepository>();
        _mockBorrowingPatternRepository = new Mock<IBorrowingPatternRepository>();
        _mockCommandLogger = new Mock<ILogger<AssignmentController>>();
        _mockGrpcServiceLogger = new Mock<ILogger<BusinessLogic.Services.BusinessLogicGrpcService>>();

        var port = FindFreePortForTestServer();
        _serverAddress = $"http://localhost:{port}";
        _testServer = await CreateAndStartTestGrpcServer(_serverAddress, port, _mockLoanRepository.Object, _mockBorrowingPatternRepository.Object, _mockGrpcServiceLogger.Object);
        _businessLogicFacade = new BusinessLogicGrpcFacade(_serverAddress);
        _sut = new GetBooksSortedByMostLoanedCommand(_businessLogicFacade, _mockCommandLogger.Object);
    }

    [TearDown]
    public async Task TearDown()
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
        return  testServer;
    }

    private static int FindFreePortForTestServer()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WithValidData_ReturnsSuccessAndBooks()
    {
        // Arrange
        var author1 = new Author { Id = 1, GivenName = "F. Scott", Surname = "Fitzgerald" };
        var author2 = new Author { Id = 2, GivenName = "George", Surname = "Orwell" };
        var book1 = new Book { Id = 1, Title = "The Great Gatsby", Author = author1 };
        var book2 = new Book { Id = 2, Title = "1984", Author = author2 };

        var loans = new[]
        {
            new Loan { Id = 1, Book = book1, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 2, Book = book1, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 3, Book = book1, Patron = new Patron { Id = 3, FirstName = "Bob", LastName = "Johnson" } },
            new Loan { Id = 4, Book = book1, Patron = new Patron { Id = 4, FirstName = "Alice", LastName = "Brown" } },
            new Loan { Id = 5, Book = book1, Patron = new Patron { Id = 5, FirstName = "Charlie", LastName = "Davis" } },
            new Loan { Id = 6, Book = book1, Patron = new Patron { Id = 6, FirstName = "Diana", LastName = "Wilson" } },
            new Loan { Id = 7, Book = book1, Patron = new Patron { Id = 7, FirstName = "Eve", LastName = "Miller" } },
            new Loan { Id = 8, Book = book1, Patron = new Patron { Id = 8, FirstName = "Frank", LastName = "Taylor" } },
            new Loan { Id = 9, Book = book1, Patron = new Patron { Id = 9, FirstName = "Grace", LastName = "Anderson" } },
            new Loan { Id = 10, Book = book1, Patron = new Patron { Id = 10, FirstName = "Henry", LastName = "Thomas" } },
            new Loan { Id = 11, Book = book2, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 12, Book = book2, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 13, Book = book2, Patron = new Patron { Id = 3, FirstName = "Bob", LastName = "Johnson" } },
            new Loan { Id = 14, Book = book2, Patron = new Patron { Id = 4, FirstName = "Alice", LastName = "Brown" } },
            new Loan { Id = 15, Book = book2, Patron = new Patron { Id = 5, FirstName = "Charlie", LastName = "Davis" } },
            new Loan { Id = 16, Book = book2, Patron = new Patron { Id = 6, FirstName = "Diana", LastName = "Wilson" } },
            new Loan { Id = 17, Book = book2, Patron = new Patron { Id = 7, FirstName = "Eve", LastName = "Miller" } },
            new Loan { Id = 18, Book = book2, Patron = new Patron { Id = 8, FirstName = "Frank", LastName = "Taylor" } },
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await _sut.TryGetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(response, Has.Length.EqualTo(2));
        Assert.That(response[0].Book.Title, Is.EqualTo("The Great Gatsby"));
        Assert.That(response[0].LoanCount, Is.EqualTo(10));
        Assert.That(response[1].Book.Title, Is.EqualTo("1984"));
        Assert.That(response[1].LoanCount, Is.EqualTo(8));

        _mockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WithNullMaxBooks_ReturnsAllBooks()
    {
        // Arrange
        var book1 = new Book { Id = 1, Title = "Book A", Author = new Author { Id = 1, GivenName = "Author", Surname = "A" } };
        var book2 = new Book { Id = 2, Title = "Book B", Author = new Author { Id = 2, GivenName = "Author", Surname = "B" } };
        var book3 = new Book { Id = 3, Title = "Book C", Author = new Author { Id = 3, GivenName = "Author", Surname = "C" } };

        var loans = new[]
        {
            new Loan { Id = 1, Book = book1, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 2, Book = book1, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 3, Book = book1, Patron = new Patron { Id = 3, FirstName = "Bob", LastName = "Johnson" } },
            new Loan { Id = 4, Book = book2, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 5, Book = book2, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 6, Book = book3, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await _sut.TryGetBooksSortedByMostLoaned(maxBooks: null);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(response, Has.Length.EqualTo(3));
        Assert.That(response[0].LoanCount, Is.EqualTo(3));
        Assert.That(response[1].LoanCount, Is.EqualTo(2));
        Assert.That(response[2].LoanCount, Is.EqualTo(1));

        _mockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WithMaxBooks_LimitsResults()
    {
        // Arrange
        var book1 = new Book { Id = 1, Title = "Book 1", Author = new Author { Id = 1, GivenName = "Author", Surname = "1" } };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = new Author { Id = 2, GivenName = "Author", Surname = "2" } };
        var book3 = new Book { Id = 3, Title = "Book 3", Author = new Author { Id = 3, GivenName = "Author", Surname = "3" } };
        var book4 = new Book { Id = 4, Title = "Book 4", Author = new Author { Id = 4, GivenName = "Author", Surname = "4" } };

        var loans = new[]
        {
            new Loan { Id = 1, Book = book1, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 2, Book = book1, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 3, Book = book1, Patron = new Patron { Id = 3, FirstName = "Bob", LastName = "Johnson" } },
            new Loan { Id = 4, Book = book1, Patron = new Patron { Id = 4, FirstName = "Alice", LastName = "Brown" } },
            new Loan { Id = 5, Book = book2, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 6, Book = book2, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 7, Book = book2, Patron = new Patron { Id = 3, FirstName = "Bob", LastName = "Johnson" } },
            new Loan { Id = 8, Book = book3, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
            new Loan { Id = 9, Book = book3, Patron = new Patron { Id = 2, FirstName = "Jane", LastName = "Smith" } },
            new Loan { Id = 10, Book = book4, Patron = new Patron { Id = 1, FirstName = "John", LastName = "Doe" } },
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await _sut.TryGetBooksSortedByMostLoaned(maxBooks: 2);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(response, Has.Length.EqualTo(2));
        Assert.That(response[0].LoanCount, Is.EqualTo(4));
        Assert.That(response[1].LoanCount, Is.EqualTo(3));

        _mockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WithEmptyResult_ReturnsSuccessAndEmptyArray()
    {
        // Arrange
        var emptyLoans = Array.Empty<Loan>();

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(emptyLoans);

        // Act
        var (success, response) = await _sut.TryGetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(response, Is.Empty);

        _mockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WhenRepositoryThrowsException_ReturnsFalseAndEmptyArray()
    {
        // Arrange
        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var (success, response) = await _sut.TryGetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(response, Is.Empty);

        _mockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task TryGetBooksSortedByMostLoaned_WhenRepositoryThrowsException_LogsError()
    {
        // Arrange
        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await _sut.TryGetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert - Verify that an error was logged (without checking the exact message)
        _mockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Constructor_WithNullBusinessLogicFacade_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _ = new GetBooksSortedByMostLoanedCommand(null!, _mockCommandLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _ = new GetBooksSortedByMostLoanedCommand(_businessLogicFacade, null!));
    }
}
