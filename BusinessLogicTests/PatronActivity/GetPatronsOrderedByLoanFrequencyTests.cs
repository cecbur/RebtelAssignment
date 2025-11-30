using BusinessModels;
using Moq;

namespace BusinessLogicTests.PatronActivity;

public class GetPatronsOrderedByLoanFrequencyTests : PatronActivityTestBase
{
    private readonly DateTime _startDate = new(2024, 1, 1);
    private readonly DateTime _endDate = new(2024, 12, 31);

    [Test]
    public async Task GetPatronsOrderedByLoanFrequency_WithMultiplePatrons_ReturnsOrderedByLoanCount()
    {
        // Arrange
        var patron1 = CreatePatron(1, "Alice", "Johnson");
        var patron2 = CreatePatron(2, "Bob", "Smith");
        var patron3 = CreatePatron(3, "Carol", "Williams");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 400);

        var loans = new List<Loan>
        {
            CreateLoan(1, book1, patron1, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19)),
            CreateLoan(2, book2, patron1, new DateTime(2024, 2, 1), new DateTime(2024, 2, 15)),
            CreateLoan(3, book1, patron1, new DateTime(2024, 3, 1), new DateTime(2024, 3, 15)),
            CreateLoan(4, book2, patron2, new DateTime(2024, 1, 10), new DateTime(2024, 1, 24)),
            CreateLoan(5, book1, patron2, new DateTime(2024, 2, 5), new DateTime(2024, 2, 19)),
            CreateLoan(6, book2, patron3, new DateTime(2024, 1, 15), new DateTime(2024, 1, 29))
        };

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await PatronActivity.GetPatronsOrderedByLoanFrequency(_startDate, _endDate);

        // Assert
        Assert.That(result.Length, Is.EqualTo(3), "Should return all 3 patrons");
        Assert.That(result[0].Patron.Id, Is.EqualTo(patron1.Id), "Most frequent patron should be first");
        Assert.That(result[0].LoanCount, Is.EqualTo(3), "First patron should have 3 loans");
        Assert.That(result[1].Patron.Id, Is.EqualTo(patron2.Id), "Second most frequent patron should be second");
        Assert.That(result[1].LoanCount, Is.EqualTo(2), "Second patron should have 2 loans");
        Assert.That(result[2].Patron.Id, Is.EqualTo(patron3.Id), "Least frequent patron should be third");
        Assert.That(result[2].LoanCount, Is.EqualTo(1), "Third patron should have 1 loan");
    }

    [Test]
    public async Task GetPatronsOrderedByLoanFrequency_WithNoLoans_ReturnsEmptyArray()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await PatronActivity.GetPatronsOrderedByLoanFrequency(_startDate, _endDate);

        // Assert
        Assert.That(result.Length, Is.EqualTo(0), "Should return empty array when there are no loans in the time period");
    }

    [Test]
    public async Task GetPatronsOrderedByLoanFrequency_WithSinglePatron_ReturnsSinglePatronLoans()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Book 1", 300);

        var loans = new List<Loan>
        {
            CreateLoan(1, book, patron, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19)),
            CreateLoan(2, book, patron, new DateTime(2024, 2, 1), new DateTime(2024, 2, 15))
        };

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await PatronActivity.GetPatronsOrderedByLoanFrequency(_startDate, _endDate);

        // Assert
        Assert.That(result.Length, Is.EqualTo(1), "Should return single patron");
        Assert.That(result[0].Patron.Id, Is.EqualTo(patron.Id), "Returned patron should match the only patron");
        Assert.That(result[0].LoanCount, Is.EqualTo(2), "Patron should have 2 loans");
    }

    [Test]
    public async Task GetPatronsOrderedByLoanFrequency_WithEqualLoanCounts_MaintainsStableOrder()
    {
        // Arrange
        var patron1 = CreatePatron(1, "Alice", "Johnson");
        var patron2 = CreatePatron(2, "Bob", "Smith");

        var book = CreateBook(1, "Book 1", 300);

        var loans = new List<Loan>
        {
            CreateLoan(1, book, patron1, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19)),
            CreateLoan(2, book, patron1, new DateTime(2024, 2, 1), new DateTime(2024, 2, 15)),
            CreateLoan(3, book, patron2, new DateTime(2024, 1, 10), new DateTime(2024, 1, 24)),
            CreateLoan(4, book, patron2, new DateTime(2024, 2, 5), new DateTime(2024, 2, 19))
        };

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await PatronActivity.GetPatronsOrderedByLoanFrequency(_startDate, _endDate);

        // Assert
        Assert.That(result.Length, Is.EqualTo(2), "Should return both patrons with equal loan counts");
        Assert.That(result[0].LoanCount, Is.EqualTo(2), "First patron should have 2 loans");
        Assert.That(result[1].LoanCount, Is.EqualTo(2), "Second patron should have 2 loans");
    }

    [Test]
    public void GetPatronsOrderedByLoanFrequency_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await PatronActivity.GetPatronsOrderedByLoanFrequency(_startDate, _endDate));
    }

    [Test]
    public async Task GetPatronsOrderedByLoanFrequency_WithStartDateAfterEndDate_ReturnsEmptyArray()
    {
        // Arrange
        var invalidStartDate = new DateTime(2024, 12, 31);
        var invalidEndDate = new DateTime(2024, 1, 1);

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(invalidStartDate, invalidEndDate))
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await PatronActivity.GetPatronsOrderedByLoanFrequency(invalidStartDate, invalidEndDate);

        // Assert
        Assert.That(result.Length, Is.EqualTo(0), "Should return empty array when start date is after end date");
    }
}
