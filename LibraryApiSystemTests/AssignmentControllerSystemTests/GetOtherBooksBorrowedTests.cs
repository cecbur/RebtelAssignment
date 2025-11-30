using System.Net;
using System.Net.Http.Json;
using LibraryApi.DTOs;
using TestData;

namespace LibraryApiSystemTests.AssignmentControllerSystemTests;

/// <summary>
/// End-to-end HTTP system tests for GetOtherBooksBorrowed endpoint.
/// Uses real HTTP calls and a real SQL Server database (via Testcontainers).
/// Nothing is mocked - tests the full stack from HTTP request to database.
/// </summary>
[TestFixture]
public class GetOtherBooksBorrowedTests : AssignmentControllerSystemTestBase
{

    [Test]
    public async Task GetOtherBooksBorrowed_WithBorrowingPattern_ReturnsAssociatedBooksOrderedByFrequency()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");

        // Create main book and associated books
        var mainBook = await _testData.CreateBook("Harry Potter", author.Id, "978-0000000001");
        var lotrBook = await _testData.CreateBook("Lord of the Rings", author.Id, "978-0000000002");
        var hobbitBook = await _testData.CreateBook("The Hobbit", author.Id, "978-0000000003");
        var narnia = await _testData.CreateBook("Chronicles of Narnia", author.Id, "978-0000000004");

        // Create patrons
        var patron1 = await _testData.CreatePatron("Alice", "Reader", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Reader", "bob@test.com");
        var patron3 = await _testData.CreatePatron("Charlie", "Reader", "charlie@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Create borrowing pattern:
        // - All 3 patrons borrow Harry Potter (main book) = 3 loans
        // - All 3 patrons also borrow LOTR = 3 loans (ratio 3/3 = 1.0)
        // - 2 patrons borrow Hobbit = 2 loans (ratio 2/3 = 0.67)
        // - 1 patron borrows Narnia = 1 loan (filtered out - needs > 1)

        await _testData.CreateLoans([
            // All patrons borrow Harry Potter
            new TestDataGenerator.LoanData(mainBook.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron2.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron3.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),

            // All patrons also borrow LOTR
            new TestDataGenerator.LoanData(lotrBook.Id, patron1.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(lotrBook.Id, patron2.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(lotrBook.Id, patron3.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),

            // 2 patrons borrow Hobbit
            new TestDataGenerator.LoanData(hobbitBook.Id, patron1.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(hobbitBook.Id, patron2.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),

            // Only 1 patron borrows Narnia (should be filtered out)
            new TestDataGenerator.LoanData(narnia.Id, patron1.Id, baseDate.AddDays(60), baseDate.AddDays(74), baseDate.AddDays(70), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{mainBook.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "HTTP response should be 200 OK");

        var associatedBooks = await response.Content.ReadFromJsonAsync<BookFrequencyResponse[]>();
        Assert.That(associatedBooks, Is.Not.Null, "Response body should not be null");
        Assert.That(associatedBooks, Has.Length.EqualTo(2), "Should return 2 associated books (LOTR and Hobbit, not Narnia with only 1 loan)");

        // Verify books are ordered by frequency ratio (descending)
        Assert.That(associatedBooks![0].AssociatedBook.Title, Is.EqualTo(lotrBook.Title), "First book should be LOTR (highest ratio)");
        Assert.That(associatedBooks[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(1.0).Within(0.01), "LOTR ratio should be 3/3 = 1.0");

        Assert.That(associatedBooks[1].AssociatedBook.Title, Is.EqualTo(hobbitBook.Title), "Second book should be Hobbit");
        Assert.That(associatedBooks[1].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.67).Within(0.01), "Hobbit ratio should be 2/3 = 0.67");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNoAssociatedBooks_ReturnsEmptyArray()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var mainBook = await _testData.CreateBook("Lonely Book", author.Id, "978-0000000001");
        var otherBook = await _testData.CreateBook("Unrelated Book", author.Id, "978-0000000002");

        var patron1 = await _testData.CreatePatron("Patron", "One", "patron1@test.com");
        var patron2 = await _testData.CreatePatron("Patron", "Two", "patron2@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Patron1 borrows main book, Patron2 borrows unrelated book
        // No overlap in borrowing patterns
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(mainBook.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(otherBook.Id, patron2.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{mainBook.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var associatedBooks = await response.Content.ReadFromJsonAsync<BookFrequencyResponse[]>();
        Assert.That(associatedBooks, Is.Not.Null);
        Assert.That(associatedBooks, Is.Empty, "Should return empty array when no other books were borrowed by same patrons");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithMultipleBorrowsOfSameBook_CountsAllLoans()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var mainBook = await _testData.CreateBook("Main Book", author.Id, "978-0000000001");
        var popularBook = await _testData.CreateBook("Very Popular Book", author.Id, "978-0000000002");

        var patron = await _testData.CreatePatron("Avid", "Reader", "avid@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Patron borrows main book once, but borrows popular book multiple times
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(mainBook.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),

            new TestDataGenerator.LoanData(popularBook.Id, patron.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(popularBook.Id, patron.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(popularBook.Id, patron.Id, baseDate.AddDays(60), baseDate.AddDays(74), baseDate.AddDays(70), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{mainBook.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var associatedBooks = await response.Content.ReadFromJsonAsync<BookFrequencyResponse[]>();
        Assert.That(associatedBooks, Is.Not.Null);
        Assert.That(associatedBooks, Has.Length.EqualTo(1));
        Assert.That(associatedBooks![0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(3.0).Within(0.01),
            "Ratio should be 3/1 = 3.0 (popular book borrowed 3 times, main book borrowed once)");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNonExistentBookId_ReturnsInternalServerError()
    {
        // Arrange
        var nonExistentBookId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{nonExistentBookId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError),
            "Should return 500 for non-existent book ID");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_FiltersOutMainBook_DoesNotReturnMainBookInResults()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var mainBook = await _testData.CreateBook("Main Book", author.Id, "978-0000000001");
        var otherBook = await _testData.CreateBook("Other Book", author.Id, "978-0000000002");

        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Patron borrows main book multiple times and other book multiple times
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(mainBook.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(otherBook.Id, patron.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(otherBook.Id, patron.Id, baseDate.AddDays(60), baseDate.AddDays(74), baseDate.AddDays(70), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{mainBook.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var associatedBooks = await response.Content.ReadFromJsonAsync<BookFrequencyResponse[]>();
        Assert.That(associatedBooks, Is.Not.Null);
        Assert.That(associatedBooks, Has.Length.EqualTo(1), "Should return only 1 book (other book, not main book itself)");
        Assert.That(associatedBooks![0].AssociatedBook.Id, Is.Not.EqualTo(mainBook.Id), "Main book should not appear in results");
        Assert.That(associatedBooks[0].AssociatedBook.Id, Is.EqualTo(otherBook.Id), "Should return the other book");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithComplexBorrowingPatterns_ReturnsCorrectFrequencyRatios()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var mainBook = await _testData.CreateBook("Main Book", author.Id, "978-0000000001");
        var book2 = await _testData.CreateBook("Book 2", author.Id, "978-0000000002");
        var book3 = await _testData.CreateBook("Book 3", author.Id, "978-0000000003");

        var patron1 = await _testData.CreatePatron("Patron", "One", "patron1@test.com");
        var patron2 = await _testData.CreatePatron("Patron", "Two", "patron2@test.com");
        var patron3 = await _testData.CreatePatron("Patron", "Three", "patron3@test.com");
        var patron4 = await _testData.CreatePatron("Patron", "Four", "patron4@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Main book: borrowed by 4 patrons (4 total loans)
        // Book 2: borrowed by 3 of those patrons (3 loans) -> ratio 3/4 = 0.75
        // Book 3: borrowed by 2 of those patrons (2 loans) -> ratio 2/4 = 0.5
        await _testData.CreateLoans([
            // All 4 patrons borrow main book
            new TestDataGenerator.LoanData(mainBook.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron2.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron3.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(mainBook.Id, patron4.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),

            // 3 patrons borrow book 2
            new TestDataGenerator.LoanData(book2.Id, patron1.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(book2.Id, patron2.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(book2.Id, patron3.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),

            // 2 patrons borrow book 3
            new TestDataGenerator.LoanData(book3.Id, patron1.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(book3.Id, patron2.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true)
        ]);

        // Act
        var response = await _client.GetAsync($"/api/Assignment/other-books-borrowed/{mainBook.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var associatedBooks = await response.Content.ReadFromJsonAsync<BookFrequencyResponse[]>();
        Assert.That(associatedBooks, Is.Not.Null);
        Assert.That(associatedBooks, Has.Length.EqualTo(2));

        // Verify ordering by frequency ratio (descending)
        Assert.That(associatedBooks![0].AssociatedBook.Title, Is.EqualTo(book2.Title));
        Assert.That(associatedBooks[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.75).Within(0.01));

        Assert.That(associatedBooks[1].AssociatedBook.Title, Is.EqualTo(book3.Title));
        Assert.That(associatedBooks[1].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.5).Within(0.01));
    }
}
