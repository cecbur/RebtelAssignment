using BusinessModels;
using Moq;

namespace BusinessLogic.Tests.InventoryInsights.UserActivity;

public class GetPagesPerDayByUserTests : UserActivityTestBase
{
    [Fact]
    public async Task WithCompletedLoans_CalculatesCorrectPagesPerDay()
    {
        // Arrange
        var patron1 = CreatePatron(1, "Alice", "Johnson");
        var patron2 = CreatePatron(2, "Bob", "Smith");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 200);

        var loans = new List<Loan>
        {
            // Patron1: 300 pages in 10 days = 30 pages/day
            CreateLoan(1, book1, patron1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Patron2: 200 pages in 5 days = 40 pages/day
            CreateLoan(2, book2, patron2, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19),
                returnDate: new DateTime(2024, 1, 10), isReturned: true)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(patron1));
        Assert.True(result.ContainsKey(patron2));
        Assert.Equal(30.0, result[patron1]!.Value);
        Assert.Equal(40.0, result[patron2]!.Value);
    }

    [Fact]
    public async Task WithOverlappingLoans_MergesTimeIntervalsCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 200);

        var loans = new List<Loan>
        {
            // First loan: Jan 1 to Jan 11 (10 days)
            CreateLoan(1, book1, patron, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Second loan: Jan 5 to Jan 15 (overlaps with first by 6 days)
            // Merged time: Jan 1 to Jan 15 (14 days)
            CreateLoan(2, book2, patron, new DateTime(2024, 1, 5), new DateTime(2024, 1, 20),
                returnDate: new DateTime(2024, 1, 15), isReturned: true)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Total pages: 300 + 200 = 500
        // Total time (merged): 14 days
        // Pages per day: 500 / 14 ≈ 35.71
        Assert.Equal(500.0 / 14.0, result[patron]!.Value, precision: 2);
    }

    [Fact]
    public async Task WithNonOverlappingLoans_SumsTimeCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 200);

        var loans = new List<Loan>
        {
            // First loan: Jan 1 to Jan 10 (9 days)
            CreateLoan(1, book1, patron, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 10), isReturned: true),
            // Second loan: Jan 20 to Jan 25 (5 days)
            // Total time: 9 + 5 = 14 days
            CreateLoan(2, book2, patron, new DateTime(2024, 1, 20), new DateTime(2024, 1, 30),
                returnDate: new DateTime(2024, 1, 25), isReturned: true)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Total pages: 300 + 200 = 500
        // Total time: 14 days
        // Pages per day: 500 / 14 ≈ 35.71
        Assert.Equal(500.0 / 14.0, result[patron]!.Value, precision: 2);
    }

    [Fact]
    public async Task WithIncompleteLoans_SkipsThoseLoans()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 200);

        var loans = new List<Loan>
        {
            // Completed loan
            CreateLoan(1, book1, patron, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Incomplete loan (no return date)
            CreateLoan(2, book2, patron, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19),
                returnDate: null, isReturned: false)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Only the completed loan counts: 300 pages in 10 days = 30 pages/day
        Assert.Equal(30.0, result[patron]!.Value);
    }

    [Fact]
    public async Task WithBooksWithoutPageCount_SkipsThoseLoans()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", numberOfPages: null);

        var loans = new List<Loan>
        {
            // Loan with page count
            CreateLoan(1, book1, patron, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Loan without page count
            CreateLoan(2, book2, patron, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19),
                returnDate: new DateTime(2024, 1, 10), isReturned: true)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Only the loan with page count: 300 pages in 10 days = 30 pages/day
        Assert.Equal(30.0, result[patron]!.Value);
    }

    [Fact]
    public async Task WithNoCompletedLoans_ExcludesPatron()
    {
        // Arrange
        var patron1 = CreatePatron(1, "Alice", "Johnson");
        var patron2 = CreatePatron(2, "Bob", "Smith");

        var book1 = CreateBook(1, "Book 1", 300);
        var book2 = CreateBook(2, "Book 2", 200);

        var loans = new List<Loan>
        {
            // Patron1 with completed loan
            CreateLoan(1, book1, patron1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Patron2 with incomplete loan
            CreateLoan(2, book2, patron2, new DateTime(2024, 1, 5), new DateTime(2024, 1, 19),
                returnDate: null, isReturned: false)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron1));
        Assert.False(result.ContainsKey(patron2));
    }

    [Fact]
    public async Task WithNoLoans_ReturnsEmptyDictionary()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task WithMultiplePatrons_CalculatesIndependently()
    {
        // Arrange
        var patron1 = CreatePatron(1, "Alice", "Johnson");
        var patron2 = CreatePatron(2, "Bob", "Smith");
        var patron3 = CreatePatron(3, "Carol", "Williams");

        var book = CreateBook(1, "Book 1", 400);

        var loans = new List<Loan>
        {
            // Patron1: 400 pages in 10 days = 40 pages/day
            CreateLoan(1, book, patron1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15),
                returnDate: new DateTime(2024, 1, 11), isReturned: true),
            // Patron2: 400 pages in 20 days = 20 pages/day
            CreateLoan(2, book, patron2, new DateTime(2024, 1, 1), new DateTime(2024, 1, 25),
                returnDate: new DateTime(2024, 1, 21), isReturned: true),
            // Patron3: 400 pages in 5 days = 80 pages/day
            CreateLoan(3, book, patron3, new DateTime(2024, 1, 10), new DateTime(2024, 1, 20),
                returnDate: new DateTime(2024, 1, 15), isReturned: true)
        };

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await UserActivity.GetPagesPerDayByPatron();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(40.0, result[patron1]!.Value);
        Assert.Equal(20.0, result[patron2]!.Value);
        Assert.Equal(80.0, result[patron3]!.Value);
    }
}
