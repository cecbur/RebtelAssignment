using BusinessLogicContracts.Interfaces;
using BusinessLogicGrpcClient;
using BusinessModels;
using DataStorageContracts.Dto;
using LibraryApi.Commands.AssignmentCommands;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

[TestFixture]
public class GetOtherBooksBorrowedCommandTests : DataStorageMockGrpcTestFixtureBase
{
    private Mock<ILogger<GetOtherBooksBorrowedCommand>> _mockCommandLogger = null!;
    private GetOtherBooksBorrowedCommand _sut = null!;
    private IBusinessLogicFacade _businessLogicFacade = null!;
    private TestDataBuilder _testDataBuilder = null!;

    [SetUp]
    public async Task SetUp()
    {
        _testDataBuilder = new TestDataBuilder();
        await SetUpGrpcServer();

        _mockCommandLogger = new Mock<ILogger<GetOtherBooksBorrowedCommand>>();
        _businessLogicFacade = new BusinessLogicGrpcFacade(ServerAddress);
        _sut = new GetOtherBooksBorrowedCommand(_businessLogicFacade, _mockCommandLogger.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        await TearDownGrpcServer();
    }

    [Test]
    public void Constructor_WithNullBusinessLogicFacade_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _ = new GetOtherBooksBorrowedCommand(null!, _mockCommandLogger.Object),
            "Constructor should throw ArgumentNullException when businessLogicFacade is null");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _ = new GetOtherBooksBorrowedCommand(_businessLogicFacade, null!),
            "Constructor should throw ArgumentNullException when logger is null");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithValidBookId_ReturnsSuccessAndAssociatedBooks()
    {
        // Arrange
        const int targetBookId = 1;
        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var targetBook = _testDataBuilder.CreateBook(targetBookId, "Target Book", author);
        var associatedBook1 = _testDataBuilder.CreateBook(2, "Associated Book 1", author);
        var associatedBook2 = _testDataBuilder.CreateBook(3, "Associated Book 2", author);

        // Create patrons who borrowed the target book
        var patron1 = _testDataBuilder.CreatePatron(1, "John", "Doe");
        var patron2 = _testDataBuilder.CreatePatron(2, "Jane", "Smith");

        // Target book loans (2 loans total)
        var targetLoan1 = _testDataBuilder.CreateLoan(1, targetBook, patron1);
        var targetLoan2 = _testDataBuilder.CreateLoan(3, targetBook, patron2);

        // Mock the repository calls
        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(targetBookId))
            .ReturnsAsync(new[] { targetLoan1, targetLoan2 });

        // Associated books data (each book borrowed at least twice)
        var associatedBooksData = new AssociatedBooks
        {
            Book = targetBook,
            Associated = new[]
            {
                new AssociatedBooks.BookCount { Book = associatedBook1, Count = 2 },
                new AssociatedBooks.BookCount { Book = associatedBook2, Count = 3 }
            }
        };

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(targetBookId))
            .ReturnsAsync(associatedBooksData);

        // Act
        var (success, response) = await _sut.GetOtherBooksBorrowed(targetBookId);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with valid bookId");
        Assert.That(response, Is.Not.Empty, "Response should contain associated books");
        Assert.That(response.Length, Is.EqualTo(2), "Should return 2 associated books");

        var book1Response = response.FirstOrDefault(r => r.AssociatedBook.Id == 2);
        Assert.That(book1Response, Is.Not.Null, "Should contain associated book 1");
        Assert.That(book1Response!.AssociatedBook.Title, Is.EqualTo("Associated Book 1"), "Should have correct title");

        var book2Response = response.FirstOrDefault(r => r.AssociatedBook.Id == 3);
        Assert.That(book2Response, Is.Not.Null, "Should contain associated book 2");
        Assert.That(book2Response!.AssociatedBook.Title, Is.EqualTo("Associated Book 2"), "Should have correct title");

        MockLoanRepository.Verify(r => r.GetLoansByBookId(targetBookId), Times.Once);
        MockBorrowingPatternRepository.Verify(r => r.GetOtherBooksBorrowed(targetBookId), Times.Once);
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNoAssociatedBooks_ReturnsSuccessWithEmptyArray()
    {
        // Arrange
        const int targetBookId = 1;
        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var targetBook = _testDataBuilder.CreateBook(targetBookId, "Target Book", author);
        var patron = _testDataBuilder.CreatePatron(1, "John", "Doe");

        // Only loan of the target book
        var loan = _testDataBuilder.CreateLoan(1, targetBook, patron);

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(targetBookId))
            .ReturnsAsync(new[] { loan });

        // No associated books (empty array)
        var associatedBooksData = new AssociatedBooks
        {
            Book = targetBook,
            Associated = Array.Empty<AssociatedBooks.BookCount>()
        };

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(targetBookId))
            .ReturnsAsync(associatedBooksData);

        // Act
        var (success, response) = await _sut.GetOtherBooksBorrowed(targetBookId);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed even with no associated books");
        Assert.That(response, Is.Empty, "Response should be empty when no other books were borrowed");

        MockLoanRepository.Verify(r => r.GetLoansByBookId(targetBookId), Times.Once);
        MockBorrowingPatternRepository.Verify(r => r.GetOtherBooksBorrowed(targetBookId), Times.Once);
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WhenRepositoryThrowsException_ReturnsFalseAndEmptyResponse()
    {
        // Arrange
        const int bookId = 1;

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(bookId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var (success, response) = await _sut.GetOtherBooksBorrowed(bookId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when repository throws exception");
        Assert.That(response, Is.Empty, "Response should be empty when operation fails");

        MockLoanRepository.Verify(r => r.GetLoansByBookId(bookId), Times.Once);
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WhenRepositoryThrowsException_LogsError()
    {
        // Arrange
        const int bookId = 1;

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(bookId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await _sut.GetOtherBooksBorrowed(bookId);

        // Assert - Verify that an error was logged
        _mockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once, "Error should be logged when repository throws exception");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithValidBookId_LogsInformation()
    {
        // Arrange
        const int bookId = 1;
        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var book = _testDataBuilder.CreateBook(bookId, "Test Book", author);
        var patron = _testDataBuilder.CreatePatron(1, "John", "Doe");
        var loan = _testDataBuilder.CreateLoan(1, book, patron);

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(bookId))
            .ReturnsAsync(new[] { loan });

        var associatedBooksData = new AssociatedBooks
        {
            Book = book,
            Associated = Array.Empty<AssociatedBooks.BookCount>()
        };

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(bookId))
            .ReturnsAsync(associatedBooksData);

        // Act
        await _sut.GetOtherBooksBorrowed(bookId);

        // Assert - Verify that information was logged
        _mockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce, "Information should be logged during successful operation");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithZeroBookId_ReturnsFailure()
    {
        // Arrange
        const int zeroBookId = 0;

        // Act
        var (success, response) = await _sut.GetOtherBooksBorrowed(zeroBookId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when bookId is 0");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNegativeBookId_ReturnsFailure()
    {
        // Arrange
        const int negativeBookId = -1;

        // Act
        var (success, response) = await _sut.GetOtherBooksBorrowed(negativeBookId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when bookId is negative");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithInvalidBookId_LogsWarning()
    {
        // Arrange
        const int invalidBookId = -1;

        // Act
        await _sut.GetOtherBooksBorrowed(invalidBookId);

        // Assert - Verify that a warning was logged
        _mockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once, "Warning should be logged when validation fails");
    }
}
