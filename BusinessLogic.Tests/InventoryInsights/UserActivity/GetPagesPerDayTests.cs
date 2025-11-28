using BusinessModels;
using Moq;
using Xunit;

namespace BusinessLogic.Tests.InventoryInsights.UserActivity;

public class GetPagesPerDayTests : UserActivityTestBase
{
    [Fact]
    public async Task WithReturnedLoan_CalculatesPagesPerDay()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Test Book", 300);

        // Loan: 300 pages from Jan 1 to Jan 11 = 10 days = 30 pages/day
        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 11),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await UserActivity.GetPagesPerDay(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30.0, result.Value, precision: 2);
    }

    [Fact]
    public async Task WithNonReturnedLoan_ReturnsNull()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Test Book", 300);

        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 15),
            returnDate: null,
            isReturned: false);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await UserActivity.GetPagesPerDay(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithReturnedLoanSameDay_CalculatesCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Short Read", 100);

        // Returned same day - 0 days
        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1, 9, 0, 0),
            new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 1, 17, 0, 0),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await UserActivity.GetPagesPerDay(1);

        // Assert
        Assert.NotNull(result);
        // 8 hours = 0.333... days, so 100 pages / 0.333... = ~300 pages/day
        Assert.True(result.Value > 0);
    }

    [Fact]
    public async Task WithLongLoan_CalculatesCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Long Book", 1000);

        // 1000 pages over 50 days = 20 pages/day
        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 3, 15),
            returnDate: new DateTime(2024, 2, 20),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await UserActivity.GetPagesPerDay(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20.0, result.Value, precision: 2);
    }

    [Fact]
    public async Task WithBookWithoutPageCount_ReturnsNull()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Book Without Pages", numberOfPages: null);

        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 11),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await UserActivity.GetPagesPerDay(1);

        // Assert
        Assert.Null(result);
    }
}
