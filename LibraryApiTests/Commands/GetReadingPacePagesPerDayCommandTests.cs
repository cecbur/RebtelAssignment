using BusinessLogicContracts.Interfaces;
using BusinessLogicGrpcClient;
using BusinessModels;
using LibraryApi.Commands.AssignmentCommands;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

[TestFixture]
public class GetReadingPacePagesPerDayCommandTests : DataStorageMockGrpcTestFixtureBase
{
    private Mock<ILogger<GetReadingPacePagesPerDayCommand>> _mockCommandLogger = null!;
    private GetReadingPacePagesPerDayCommand _sut = null!;
    private IBusinessLogicFacade _businessLogicFacade = null!;
    private TestDataBuilder _testDataBuilder = null!;

    [SetUp]
    public async Task SetUp()
    {
        _testDataBuilder = new TestDataBuilder();
        await SetUpGrpcServer();

        _mockCommandLogger = new Mock<ILogger<GetReadingPacePagesPerDayCommand>>();
        _businessLogicFacade = new BusinessLogicGrpcFacade(ServerAddress);
        _sut = new GetReadingPacePagesPerDayCommand(_businessLogicFacade, _mockCommandLogger.Object);
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
            _ = new GetReadingPacePagesPerDayCommand(null!, _mockCommandLogger.Object),
            "Constructor should throw ArgumentNullException when businessLogicFacade is null");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _ = new GetReadingPacePagesPerDayCommand(_businessLogicFacade, null!),
            "Constructor should throw ArgumentNullException when logger is null");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithReturnedLoan_ReturnsSuccessAndPagesPerDay()
    {
        // Arrange
        const int loanId = 1;
        const int numberOfPages = 300;
        var loanDate = new DateTime(2024, 1, 1);
        var returnDate = new DateTime(2024, 1, 11); // 10 days
        const double expectedPagesPerDay = 30.0; // 300 pages / 10 days

        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var book = _testDataBuilder.CreateBook(1, "Test Book", author);
        book.NumberOfPages = numberOfPages;
        var patron = _testDataBuilder.CreatePatron(1, "Test", "Patron");
        var loan = _testDataBuilder.CreateLoan(loanId, book, patron);
        loan.LoanDate = loanDate;
        loan.ReturnDate = returnDate;

        MockLoanRepository
            .Setup(r => r.GetLoanById(loanId))
            .ReturnsAsync(loan);

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(loanId);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with returned loan");
        Assert.That(response.LoanId, Is.EqualTo(loanId), "Response should have correct loan ID");
        Assert.That(response.PagesPerDay, Is.Not.Null, "PagesPerDay should not be null for returned loan");
        Assert.That(response.PagesPerDay, Is.EqualTo(expectedPagesPerDay), "PagesPerDay should be calculated correctly");
        Assert.That(response.Message, Is.Null, "Message should be null for successful calculation");

        MockLoanRepository.Verify(r => r.GetLoanById(loanId), Times.Once);
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithNonReturnedLoan_ReturnsSuccessWithMessage()
    {
        // Arrange
        const int loanId = 1;
        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var book = _testDataBuilder.CreateBook(1, "Test Book", author);
        book.NumberOfPages = 300;
        var patron = _testDataBuilder.CreatePatron(1, "Test", "Patron");
        var loan = _testDataBuilder.CreateLoan(loanId, book, patron);
        loan.LoanDate = new DateTime(2024, 1, 1);
        loan.ReturnDate = null; // Not returned yet

        MockLoanRepository
            .Setup(r => r.GetLoanById(loanId))
            .ReturnsAsync(loan);

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(loanId);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed even when loan not returned");
        Assert.That(response.LoanId, Is.EqualTo(loanId), "Response should have correct loan ID");
        Assert.That(response.PagesPerDay, Is.Null, "PagesPerDay should be null for non-returned loan");
        Assert.That(response.Message, Does.Contain("not").And.Contain("returned").IgnoreCase,
            "Message should indicate loan not returned");

        MockLoanRepository.Verify(r => r.GetLoanById(loanId), Times.Once);
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithBookWithoutPages_ReturnsSuccessWithMessage()
    {
        // Arrange
        const int loanId = 1;
        var author = _testDataBuilder.CreateAuthor(1, "Test", "Author");
        var book = _testDataBuilder.CreateBook(1, "Test Book", author);
        book.NumberOfPages = null; // No page count
        var patron = _testDataBuilder.CreatePatron(1, "Test", "Patron");
        var loan = _testDataBuilder.CreateLoan(loanId, book, patron);
        loan.LoanDate = new DateTime(2024, 1, 1);
        loan.ReturnDate = new DateTime(2024, 1, 11);

        MockLoanRepository
            .Setup(r => r.GetLoanById(loanId))
            .ReturnsAsync(loan);

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(loanId);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed even when book has no page count");
        Assert.That(response.LoanId, Is.EqualTo(loanId), "Response should have correct loan ID");
        Assert.That(response.PagesPerDay, Is.Null, "PagesPerDay should be null when book has no page count");
        Assert.That(response.Message, Does.Contain("not").And.Contain("returned").IgnoreCase,
            "Message should indicate loan not returned");

        MockLoanRepository.Verify(r => r.GetLoanById(loanId), Times.Once);
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WhenRepositoryThrowsException_ReturnsFalseAndResponse()
    {
        // Arrange
        const int loanId = 1;

        MockLoanRepository
            .Setup(r => r.GetLoanById(loanId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(loanId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when repository throws exception");
        Assert.That(response.LoanId, Is.EqualTo(loanId), "Response should have correct loan ID even on failure");

        MockLoanRepository.Verify(r => r.GetLoanById(loanId), Times.Once);
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WhenRepositoryThrowsException_LogsError()
    {
        // Arrange
        const int loanId = 1;

        MockLoanRepository
            .Setup(r => r.GetLoanById(loanId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await _sut.GetReadingPacePagesPerDay(loanId);

        // Assert - Verify that an error was logged (without checking the exact message)
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
    public async Task GetReadingPacePagesPerDay_WithZeroLoanId_ReturnsFailure()
    {
        // Arrange
        const int zeroLoanId = 0;

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(zeroLoanId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when loanId is 0");
        Assert.That(response.LoanId, Is.EqualTo(zeroLoanId), "Response should have correct loan ID");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoanById(It.IsAny<int>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithNegativeLoanId_ReturnsFailure()
    {
        // Arrange
        const int negativeLoanId = -1;

        // Act
        var (success, response) = await _sut.GetReadingPacePagesPerDay(negativeLoanId);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when loanId is negative");
        Assert.That(response.LoanId, Is.EqualTo(negativeLoanId), "Response should have correct loan ID");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoanById(It.IsAny<int>()), Times.Never,
            "Repository should not be called when validation fails");
    }
}
