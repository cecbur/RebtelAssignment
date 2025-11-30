using System.Net;
using System.Net.Http.Json;
using LibraryApi.DTOs;
using TestData;

namespace LibraryApiSystemTests.AssignmentControllerSystemTests;

/// <summary>
/// End-to-end HTTP system tests for GetMostActivePatrons endpoint.
/// Uses real HTTP calls and a real SQL Server database (via Testcontainers).
/// Nothing is mocked - tests the full stack from HTTP request to database.
/// </summary>
[TestFixture]
public class GetMostActivePatronsTests : AssignmentControllerSystemTestBase
{

    [Test]
    public async Task GetMostActivePatrons_WithMultiplePatrons_ReturnsPatronsOrderedByLoanCount()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book1 = await _testData.CreateBook("Book 1", author.Id, "978-0000000001");
        var book2 = await _testData.CreateBook("Book 2", author.Id, "978-0000000002");
        var book3 = await _testData.CreateBook("Book 3", author.Id, "978-0000000003");

        // Create patrons with different activity levels
        var patron1 = await _testData.CreatePatron("Alice", "Active", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Moderate", "bob@test.com");
        var patron3 = await _testData.CreatePatron("Charlie", "Low", "charlie@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Alice borrows 5 books in the time frame
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book1.Id, patron1.Id, baseDate.AddDays(1), baseDate.AddDays(15), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book2.Id, patron1.Id, baseDate.AddDays(2), baseDate.AddDays(16), baseDate.AddDays(12), true),
            new TestDataGenerator.LoanData(book3.Id, patron1.Id, baseDate.AddDays(3), baseDate.AddDays(17), baseDate.AddDays(13), true),
            new TestDataGenerator.LoanData(book1.Id, patron1.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(book2.Id, patron1.Id, baseDate.AddDays(21), baseDate.AddDays(35), baseDate.AddDays(31), true)
        ]);

        // Bob borrows 3 books in the time frame
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book1.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), baseDate.AddDays(15), true),
            new TestDataGenerator.LoanData(book2.Id, patron2.Id, baseDate.AddDays(6), baseDate.AddDays(20), baseDate.AddDays(16), true),
            new TestDataGenerator.LoanData(book3.Id, patron2.Id, baseDate.AddDays(7), baseDate.AddDays(21), baseDate.AddDays(17), true)
        ]);

        // Charlie borrows 1 book in the time frame
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book1.Id, patron3.Id, baseDate.AddDays(10), baseDate.AddDays(24), baseDate.AddDays(20), true)
        ]);

        // Act
        var startDate = baseDate;
        var endDate = baseDate.AddDays(40);
        var maxPatrons = 10;
        var response = await _client.GetAsync($"/api/Assignment/most-active-patrons?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&maxPatrons={maxPatrons}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "HTTP response should be 200 OK");

        var patrons = await response.Content.ReadFromJsonAsync<PatronLoanFrequencyResponse[]>();
        Assert.That(patrons, Is.Not.Null, "Response body should not be null");
        Assert.That(patrons, Has.Length.EqualTo(3), "Should return all 3 patrons");

        // Verify patrons are ordered by loan count (descending)
        Assert.That(patrons![0].PatronId, Is.EqualTo(patron1.Id), "First patron should be Alice (5 loans)");
        Assert.That(patrons[0].LoanCount, Is.EqualTo(5), "Alice should have 5 loans");
        Assert.That(patrons[0].PatronName, Does.Contain("Alice"), "Patron name should contain 'Alice'");

        Assert.That(patrons[1].PatronId, Is.EqualTo(patron2.Id), "Second patron should be Bob (3 loans)");
        Assert.That(patrons[1].LoanCount, Is.EqualTo(3), "Bob should have 3 loans");

        Assert.That(patrons[2].PatronId, Is.EqualTo(patron3.Id), "Third patron should be Charlie (1 loan)");
        Assert.That(patrons[2].LoanCount, Is.EqualTo(1), "Charlie should have 1 loan");
    }

    [Test]
    public async Task GetMostActivePatrons_WithMaxPatronsLimit_ReturnsLimitedResults()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Book", author.Id, "978-0000000001");

        var patron1 = await _testData.CreatePatron("Patron", "One", "patron1@test.com");
        var patron2 = await _testData.CreatePatron("Patron", "Two", "patron2@test.com");
        var patron3 = await _testData.CreatePatron("Patron", "Three", "patron3@test.com");
        var patron4 = await _testData.CreatePatron("Patron", "Four", "patron4@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Create different loan counts for each patron
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book.Id, patron1.Id, baseDate.AddDays(15), baseDate.AddDays(29), baseDate.AddDays(25), true),
            new TestDataGenerator.LoanData(book.Id, patron1.Id, baseDate.AddDays(30), baseDate.AddDays(44), baseDate.AddDays(40), true),
            new TestDataGenerator.LoanData(book.Id, patron1.Id, baseDate.AddDays(45), baseDate.AddDays(59), baseDate.AddDays(55), true),

            new TestDataGenerator.LoanData(book.Id, patron2.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book.Id, patron2.Id, baseDate.AddDays(15), baseDate.AddDays(29), baseDate.AddDays(25), true),
            new TestDataGenerator.LoanData(book.Id, patron2.Id, baseDate.AddDays(30), baseDate.AddDays(44), baseDate.AddDays(40), true),

            new TestDataGenerator.LoanData(book.Id, patron3.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book.Id, patron3.Id, baseDate.AddDays(15), baseDate.AddDays(29), baseDate.AddDays(25), true),

            new TestDataGenerator.LoanData(book.Id, patron4.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true)
        ]);

        // Act - Request only top 2 patrons
        var response = await _client.GetAsync($"/api/Assignment/most-active-patrons?startDate={baseDate:yyyy-MM-dd}&endDate={baseDate.AddDays(60):yyyy-MM-dd}&maxPatrons=2");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var patrons = await response.Content.ReadFromJsonAsync<PatronLoanFrequencyResponse[]>();
        Assert.That(patrons, Has.Length.EqualTo(2), "Should return only 2 patrons when maxPatrons=2");
        Assert.That(patrons![0].LoanCount, Is.EqualTo(4), "First patron should have 4 loans");
        Assert.That(patrons[1].LoanCount, Is.EqualTo(3), "Second patron should have 3 loans");
    }

    [Test]
    public async Task GetMostActivePatrons_WithTimeFrameFilter_ReturnsOnlyLoansInTimeFrame()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Book", author.Id, "978-0000000001");
        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        var filterStartDate = new DateTime(2024, 2, 1);
        var filterEndDate = new DateTime(2024, 2, 28);

        // Create loans: 2 before timeframe, 3 within timeframe, 1 after timeframe
        await _testData.CreateLoans([
            // Before timeframe
            new TestDataGenerator.LoanData(book.Id, patron.Id, baseDate.AddDays(-20), baseDate.AddDays(-6), baseDate.AddDays(-10), true),
            new TestDataGenerator.LoanData(book.Id, patron.Id, baseDate.AddDays(-10), baseDate.AddDays(4), baseDate.AddDays(0), true),

            // Within timeframe (Feb 1-28)
            new TestDataGenerator.LoanData(book.Id, patron.Id, filterStartDate.AddDays(1), filterStartDate.AddDays(15), filterStartDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book.Id, patron.Id, filterStartDate.AddDays(5), filterStartDate.AddDays(19), filterStartDate.AddDays(15), true),
            new TestDataGenerator.LoanData(book.Id, patron.Id, filterStartDate.AddDays(10), filterStartDate.AddDays(24), filterStartDate.AddDays(20), true),

            // After timeframe
            new TestDataGenerator.LoanData(book.Id, patron.Id, filterEndDate.AddDays(5), filterEndDate.AddDays(19), filterEndDate.AddDays(15), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/most-active-patrons?startDate={filterStartDate:yyyy-MM-dd}&endDate={filterEndDate:yyyy-MM-dd}&maxPatrons=10");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var patrons = await response.Content.ReadFromJsonAsync<PatronLoanFrequencyResponse[]>();
        Assert.That(patrons, Has.Length.EqualTo(1), "Should return 1 patron");
        Assert.That(patrons![0].LoanCount, Is.EqualTo(3), "Should count only the 3 loans within the timeframe");
    }

    [Test]
    public async Task GetMostActivePatrons_WithNoLoansInTimeFrame_ReturnsEmptyArray()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Book", author.Id, "978-0000000001");
        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");

        // Create loans outside the timeframe we'll query
        var loanDate = new DateTime(2023, 1, 1);
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, loanDate, loanDate.AddDays(14), loanDate.AddDays(10), true)
        ]);

        // Act - Query a different timeframe
        var queryStartDate = new DateTime(2024, 1, 1);
        var queryEndDate = new DateTime(2024, 12, 31);
        var response = await _client.GetAsync($"/api/Assignment/most-active-patrons?startDate={queryStartDate:yyyy-MM-dd}&endDate={queryEndDate:yyyy-MM-dd}&maxPatrons=10");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var patrons = await response.Content.ReadFromJsonAsync<PatronLoanFrequencyResponse[]>();
        Assert.That(patrons, Is.Not.Null, "Response should not be null");
        Assert.That(patrons, Is.Empty, "Should return empty array when no loans in timeframe");
    }
}
