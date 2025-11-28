using BusinessModels;
using Moq;

namespace BusinessLogic.Tests.InventoryInsights.UserActivity;

public class GetPatronLoansOrderedByFrequencyTests : UserActivityTestBase
{
    private readonly DateTime _startDate = new(2024, 1, 1);
    private readonly DateTime _endDate = new(2024, 12, 31);

    [Fact]
    public async Task WithMultiplePatrons_ReturnsOrderedByLoanCount()
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
        var result = await UserActivity.GetPatronLoansOrderedByFrequency(_startDate, _endDate);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal(patron1.Id, result[0].Patron.Id);
        Assert.Equal(3, result[0].LoanCount);
        Assert.Equal(patron2.Id, result[1].Patron.Id);
        Assert.Equal(2, result[1].LoanCount);
        Assert.Equal(patron3.Id, result[2].Patron.Id);
        Assert.Equal(1, result[2].LoanCount);
    }

    [Fact]
    public async Task WithNoLoans_ReturnsEmptyArray()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await UserActivity.GetPatronLoansOrderedByFrequency(_startDate, _endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task WithSinglePatron_ReturnsSinglePatronLoans()
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
        var result = await UserActivity.GetPatronLoansOrderedByFrequency(_startDate, _endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(patron.Id, result[0].Patron.Id);
        Assert.Equal(2, result[0].LoanCount);
    }

    [Fact]
    public async Task WithEqualLoanCounts_MaintainsStableOrder()
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
        var result = await UserActivity.GetPatronLoansOrderedByFrequency(_startDate, _endDate);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(2, result[0].LoanCount);
        Assert.Equal(2, result[1].LoanCount);
    }
}
