using BusinessLogicContracts.Interfaces;
using BusinessModels;
using LibraryApi.Commands.AssignmentCommands;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

[TestFixture]
public class GetBooksSortedByMostLoanedCommandTests : CommandTestBase<GetBooksSortedByMostLoanedCommand>
{
    protected override GetBooksSortedByMostLoanedCommand CreateSystemUnderTest(
        IBusinessLogicFacade businessLogicFacade,
        ILogger<GetBooksSortedByMostLoanedCommand> logger)
    {
        return new GetBooksSortedByMostLoanedCommand(businessLogicFacade, logger);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithValidData_ReturnsSuccessAndBooks()
    {
        // Arrange
        const int book1LoanCount = 10;
        const int book2LoanCount = 8;

        var author1 = TestDataBuilder.CreateAuthor(1, "F. Scott", "Fitzgerald");
        var author2 = TestDataBuilder.CreateAuthor(2, "George", "Orwell");
        var book1 = TestDataBuilder.CreateBook(1, "The Great Gatsby", author1);
        var book2 = TestDataBuilder.CreateBook(2, "1984", author2);

        var book1Loans = TestDataBuilder.CreateLoansForBook(book1, startId: 1, count: book1LoanCount);
        var book2Loans = TestDataBuilder.CreateLoansForBook(book2, startId: book1Loans.Max(l => l.Id)+1, count: book2LoanCount);
        var loans = book1Loans.Concat(book2Loans).ToArray();

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with valid data");
        Assert.That(response, Has.Length.EqualTo(2), "Should return exactly 2 books");
        Assert.That(response[0].Book.Title, Is.EqualTo("The Great Gatsby"), "First book should be 'The Great Gatsby' (most loaned)");
        Assert.That(response[0].LoanCount, Is.EqualTo(book1LoanCount), "First book should have 10 loans");
        Assert.That(response[1].Book.Title, Is.EqualTo("1984"), "Second book should be '1984'");
        Assert.That(response[1].LoanCount, Is.EqualTo(book2LoanCount), "Second book should have 8 loans");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithNullMaxBooks_ReturnsAllBooks()
    {
        // Arrange
        const int book1LoanCount = 3;
        const int book2LoanCount = 2;
        const int book3LoanCount = 1;

        var book1 = TestDataBuilder.CreateBook(1, "Book A", TestDataBuilder.CreateAuthor(1, "Author", "A"));
        var book2 = TestDataBuilder.CreateBook(2, "Book B", TestDataBuilder.CreateAuthor(2, "Author", "B"));
        var book3 = TestDataBuilder.CreateBook(3, "Book C", TestDataBuilder.CreateAuthor(3, "Author", "C"));

        var book1Loans = TestDataBuilder.CreateLoansForBook(book1, startId: 1, count: book1LoanCount);
        var book2Loans = TestDataBuilder.CreateLoansForBook(book2, startId: book1Loans.Max(l => l.Id)+1, count: book2LoanCount);
        var book3Loans = TestDataBuilder.CreateLoansForBook(book3, startId: book2Loans.Max(l => l.Id)+1, count: book3LoanCount);
        var loans = book1Loans.Concat(book2Loans).Concat(book3Loans).ToArray();

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2, book3 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: null);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with null maxBooks");
        Assert.That(response, Has.Length.EqualTo(3), "Should return all 3 books when maxBooks is null");
        Assert.That(response[0].LoanCount, Is.EqualTo(book1LoanCount), "First book should have 3 loans");
        Assert.That(response[1].LoanCount, Is.EqualTo(book2LoanCount), "Second book should have 2 loans");
        Assert.That(response[2].LoanCount, Is.EqualTo(book3LoanCount), "Third book should have 1 loan");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithMaxBooks_LimitsResults()
    {
        // Arrange
        const int maxBooksToReturn = 2;
        const int book1LoanCount = 4;
        const int book2LoanCount = 3;
        const int book3LoanCount = 2;
        const int book4LoanCount = 1;

        var book1 = TestDataBuilder.CreateBook(1, "Book 1", TestDataBuilder.CreateAuthor(1, "Author", "1"));
        var book2 = TestDataBuilder.CreateBook(2, "Book 2", TestDataBuilder.CreateAuthor(2, "Author", "2"));
        var book3 = TestDataBuilder.CreateBook(3, "Book 3", TestDataBuilder.CreateAuthor(3, "Author", "3"));
        var book4 = TestDataBuilder.CreateBook(4, "Book 4", TestDataBuilder.CreateAuthor(4, "Author", "4"));

        var book1Loans = TestDataBuilder.CreateLoansForBook(book1, startId: 1, count: book1LoanCount);
        var book2Loans = TestDataBuilder.CreateLoansForBook(book2, startId: book1Loans.Max(l => l.Id)+1, count: book2LoanCount);
        var book3Loans = TestDataBuilder.CreateLoansForBook(book3, startId: book2Loans.Max(l => l.Id)+1, count: book3LoanCount);
        var book4Loans = TestDataBuilder.CreateLoansForBook(book4, startId: book3Loans.Max(l => l.Id)+1, count: book4LoanCount);
        var loans = book1Loans.Concat(book2Loans).Concat(book3Loans).Concat(book4Loans).ToArray();

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2, book3, book4 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: maxBooksToReturn);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with maxBooks limit");
        Assert.That(response, Has.Length.EqualTo(maxBooksToReturn), "Should return exactly 2 books when maxBooks=2");
        Assert.That(response[0].LoanCount, Is.EqualTo(book1LoanCount), "First book should have 4 loans (highest)");
        Assert.That(response[1].LoanCount, Is.EqualTo(book2LoanCount), "Second book should have 3 loans");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithEmptyResult_ReturnsSuccessAndEmptyArray()
    {
        // Arrange
        var emptyLoans = Array.Empty<Loan>();

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(emptyLoans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed even with no loans");
        Assert.That(response, Is.Empty, "Should return empty array when there are no loans");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WhenRepositoryThrowsException_ReturnsFalseAndEmptyArray()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when repository throws exception");
        Assert.That(response, Is.Empty, "Response should be empty when operation fails");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WhenRepositoryThrowsException_LogsError()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await Sut.GetBooksSortedByMostLoaned(maxBooks: 10);

        // Assert - Verify that an error was logged (without checking the exact message)
        MockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithZeroMaxBooks_ReturnsFailure()
    {
        // Arrange
        const int zeroMaxBooks = 0;
        var book = TestDataBuilder.CreateBook(1, "Book 1", TestDataBuilder.CreateAuthor(1, "Author", "1"));
        var loans = TestDataBuilder.CreateLoansForBook(book, startId: 1, count: 5);

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: zeroMaxBooks);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when maxBooks is 0");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Never);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithNegativeMaxBooks_ReturnsFailure()
    {
        // Arrange
        const int negativeMaxBooks = -1;

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: negativeMaxBooks);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when maxBooks is negative");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Never);
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithIdenticalLoanCounts_MaintainsStableOrder()
    {
        // Arrange
        const int sameLoanCount = 5;
        var book1 = TestDataBuilder.CreateBook(1, "Book A", TestDataBuilder.CreateAuthor(1, "Author", "1"));
        var book2 = TestDataBuilder.CreateBook(2, "Book B", TestDataBuilder.CreateAuthor(2, "Author", "2"));
        var book3 = TestDataBuilder.CreateBook(3, "Book C", TestDataBuilder.CreateAuthor(3, "Author", "3"));

        var book1Loans = TestDataBuilder.CreateLoansForBook(book1, startId: 1, count: sameLoanCount);
        var book2Loans = TestDataBuilder.CreateLoansForBook(book2, startId: book1Loans.Max(l => l.Id)+1, count: sameLoanCount);
        var book3Loans = TestDataBuilder.CreateLoansForBook(book3, startId: book2Loans.Max(l => l.Id)+1, count: sameLoanCount);
        var loans = book1Loans.Concat(book2Loans).Concat(book3Loans).ToArray();

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2, book3 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetBooksSortedByMostLoaned(maxBooks: null);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with identical loan counts");
        Assert.That(response, Has.Length.EqualTo(3), "Should return all 3 books");

        // All books should have the same loan count
        Assert.That(response[0].LoanCount, Is.EqualTo(sameLoanCount), "First book should have 5 loans");
        Assert.That(response[1].LoanCount, Is.EqualTo(sameLoanCount), "Second book should have 5 loans");
        Assert.That(response[2].LoanCount, Is.EqualTo(sameLoanCount), "Third book should have 5 loans");

        MockLoanRepository.Verify(r => r.GetAllLoans(), Times.Once);
    }

}