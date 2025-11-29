using Moq;

namespace BusinessLogic.Tests.PatronActivity;

public class GetPagesPerDayTests : PatronActivityTestBase
{
    [Test]
    public async Task GetPagesPerDay_WithReturnedLoan_CalculatesPagesPerDay()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Test Book", 300);

        var loan = CreateLoan(1, book, patron,
            loanDate:  new DateTime(2024, 1, 1),
            dueDate: new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 11),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Not.Null, "Should return a value when loan is returned with page count");
        // Loan: 300 pages from Jan 1 to Jan 11 = 10 days = 30 pages/day
        Assert.That(result!.Value, Is.EqualTo(30.0).Within(0.01), "Should calculate 30 pages per day (300 pages / 10 days)");
    }

    [Test]
    public async Task GetPagesPerDay_WithNonReturnedLoan_ReturnsNull()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Test Book", 300);

        var loan = CreateLoan(1, book, patron,
            loanDate: new DateTime(2024, 1, 1),
            dueDate: new DateTime(2024, 1, 15),
            returnDate: null,
            isReturned: false);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Null, "Should return null when loan has not been returned");
    }

    [Test]
    public async Task GetPagesPerDay_WithReturnedLoanSameDay_CalculatesCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Short Read", 100);

        // Returned same day - 0 days
        var loan = CreateLoan(1, book, patron,
            loanDate: new DateTime(2024, 1, 1, 9, 0, 0),
            dueDate: new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 1, 17, 0, 0),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Not.Null, "Should return a value when loan is returned same day");
        // 8 hours = 0.333... days, so 100 pages / 0.333... = ~300 pages/day
        Assert.That(result!.Value, Is.GreaterThan(0), "Should calculate a positive pages per day value for same-day return");
    }

    [Test]
    public async Task GetPagesPerDay_WithLongLoan_CalculatesCorrectly()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Long Book", 1000);

        // 1000 pages over 50 days = 20 pages/day
        var loan = CreateLoan(1, book, patron,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 3, 15),
            returnDate: new DateTime(2024, 6, 16),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Not.Null, "Should return a value for long loan with page count");
        Assert.That(result!.Value, Is.EqualTo(6.0).Within(0.1), "Should calculate 5,99 pages per day (1000 pages / 167 days)");
    }

    [Test]
    public async Task GetPagesPerDay_WithBookWithoutPageCount_ReturnsNull()
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
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Null, "Should return null when book has no page count");
    }

    [Test]
    public void GetPagesPerDay_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetLoanById(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await PatronActivity.GetPagesPerDay(1));
    }

    [Test]
    public async Task GetPagesPerDay_WithLoanWithZeroDuration_ReturnsInfinity()
    {
        // Arrange
        var patron = CreatePatron(1, "Alice", "Johnson");
        var book = CreateBook(1, "Instant Read", 100);

        var loan = CreateLoan(1, book, patron,
            loanDate: new DateTime(2024, 1, 1, 10, 0, 0),
            dueDate: new DateTime(2024, 1, 15),
            returnDate: new DateTime(2024, 1, 1, 10, 0, 0),
            isReturned: true);

        MockLoanRepository
            .Setup(r => r.GetLoanById(1))
            .ReturnsAsync(loan);

        // Act
        var result = await PatronActivity.GetPagesPerDay(1);

        // Assert
        Assert.That(result, Is.Not.Null, "Should return a value even with zero duration");
        Assert.That(double.IsInfinity(result!.Value) || result.Value > 1000000, Is.True, "Should return infinity or very large number for instant read");
    }
}
