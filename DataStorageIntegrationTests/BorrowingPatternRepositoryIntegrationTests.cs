using DataStorage.Repositories;
using DataStorage.RepositoriesMultipleTables;

namespace DataStorageIntegrationTests;

/// <summary>
/// Integration tests for BorrowingPatternRepository using a real SQL Server database.
/// These tests use Testcontainers to spin up a SQL Server instance in Docker.
/// Tests in this fixture share database state and must NOT run in parallel.
/// </summary>
public class BorrowingPatternRepositoryIntegrationTests : RepositoryIntegrationTestBase<BorrowingPatternRepository>
{
    private BookRepository _bookRepository = null!;

    protected override BorrowingPatternRepository CreateRepository()
    {
        _bookRepository = new BookRepository(_connectionFactory);
        return new BorrowingPatternRepository(_connectionFactory, _bookRepository);
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithMultiplePatronsShareSimilarBooks_ReturnsAssociatedBooksSortedByFrequency()
    {
        // Arrange - Create test data using TestDataGenerator
        var author1 = await _testData.CreateAuthor("George", "Orwell");
        var author2 = await _testData.CreateAuthor("Aldous", "Huxley");

        var book1984 = await _testData.CreateBook("1984", author1.Id, "978-0451524935", 1949, 328);
        var bookAnimalFarm = await _testData.CreateBook("Animal Farm", author1.Id, "978-0451526342", 1945, 112);
        var bookBraveNewWorld = await _testData.CreateBook("Brave New World", author2.Id, "978-0060850524", 1932, 268);
        var bookUnrelated = await _testData.CreateBook("Unrelated Book", author2.Id, "978-1234567890", 2020, 200);

        var patron1 = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Smith", "bob@test.com");
        var patron3 = await _testData.CreatePatron("Carol", "Williams", "carol@test.com");
        var patron4 = await _testData.CreatePatron("David", "Brown", "david@test.com");

        // Create borrowing pattern:
        // - Patrons 1, 2, 3 all borrowed "1984" and "Animal Farm" (Animal Farm appears 3 times)
        // - Patrons 1, 2 also borrowed "Brave New World" (appears 2 times)
        // - Patron 4 borrowed only "Unrelated Book" (should not appear in results)
        var baseDate = new DateTime(2024, 1, 1);

        await _testData.CreateLoans([
            // Patron 1 borrows: 1984, Animal Farm, Brave New World
            new TestDataGenerator.LoanData(book1984.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(bookAnimalFarm.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(bookBraveNewWorld.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),

            // Patron 2 borrows: 1984, Animal Farm, Brave New World
            new TestDataGenerator.LoanData(book1984.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), baseDate.AddDays(15), true),
            new TestDataGenerator.LoanData(bookAnimalFarm.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), baseDate.AddDays(15), true),
            new TestDataGenerator.LoanData(bookBraveNewWorld.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), baseDate.AddDays(15), true),

            // Patron 3 borrows: 1984, Animal Farm (but NOT Brave New World)
            new TestDataGenerator.LoanData(book1984.Id, patron3.Id, baseDate.AddDays(10), baseDate.AddDays(24), baseDate.AddDays(20), true),
            new TestDataGenerator.LoanData(bookAnimalFarm.Id, patron3.Id, baseDate.AddDays(10), baseDate.AddDays(24), baseDate.AddDays(20), true),

            // Patron 4 borrows only unrelated book (should not affect results)
            new TestDataGenerator.LoanData(bookUnrelated.Id, patron4.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true)
        ]);

                
        // Assert test data was created correctly
        Assert.That(book1984.Id, Is.GreaterThan(0), "Test data could not be created! Book 1984 should have generated ID");
        Assert.That(book1984.Title, Is.EqualTo("1984"), "Test data could not be created! Book title should match database value");
        Assert.That(book1984.NumberOfPages, Is.EqualTo(328), "Test data could not be created! NumberOfPages should match database value");
        Assert.That(patron1.Email, Is.EqualTo("alice@test.com"), "Test data could not be created! Patron email should match database value");
        
        // Act - Call the repository method
        var result = await _sut.GetOtherBooksBorrowed(book1984.Id);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.Book, Is.Not.Null, "Main book should not be null");
        Assert.That(result.Book.Title, Is.EqualTo(book1984.Title), "Main book title should match");
        Assert.That(result.Book.Id, Is.EqualTo(book1984.Id), "Main book ID should match");

        // Should return 2 associated books (Animal Farm and Brave New World), sorted by borrow count DESC
        Assert.That(result.Associated, Has.Length.EqualTo(2), "Should return exactly 2 associated books");

        // First associated book should be Animal Farm (borrowed 3 times)
        var firstBook = result.Associated[0];
        Assert.That(firstBook.Book.Title, Is.EqualTo(bookAnimalFarm.Title),
            "First associated book should be 'Animal Farm' (highest frequency)");
        Assert.That(firstBook.Book.Id, Is.EqualTo(bookAnimalFarm.Id),
            "First associated book Animal Farm has wrong ID");
        Assert.That(firstBook.Count, Is.EqualTo(3),
            "Animal Farm should have borrow count of 3");

        // Second associated book should be Brave New World (borrowed 2 times)
        var secondBook = result.Associated[1];
        Assert.That(secondBook.Book.Title, Is.EqualTo(bookBraveNewWorld.Title),
            "Second associated book should be 'Brave New World'");
        Assert.That(secondBook.Book.Id, Is.EqualTo(bookBraveNewWorld.Id),
            "Second associated book Brave New World has wrong ID");
        Assert.That(secondBook.Count, Is.EqualTo(2),
            "Brave New World should have borrow count of 2");

        // Verify unrelated book is not included
        Assert.That(result.Associated.Any(ab => ab.Book.Title == "Unrelated Book"), Is.False,
            "Unrelated book should not be in associated books");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNoAssociatedBooks_ReturnsEmptyAssociatedArray()
    {
        // Arrange - Book with no other borrowing patterns
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Lonely Book", author.Id, "978-9999999999");
        var patron = await _testData.CreatePatron("Solo", "Reader", "solo@test.com");

        // Only one patron borrowed this book, and no other books
        var baseDate = DateTime.Now;
        _ = await _testData.CreateLoan(book.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true);

        Assert.Multiple(() =>
        {
            // Assert test data was created correctly
            Assert.That(book.Title, Is.EqualTo("Lonely Book"), "Test data could not be created! Book title should match database value");
            Assert.That(patron.FirstName, Is.EqualTo("Solo"), "Test data could not be created! Patron name should match database value");
        });

        // Act
        var result = await _sut.GetOtherBooksBorrowed(book.Id);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.Book.Title, Is.EqualTo(book.Title), "Main book title should match");
        Assert.That(result.Book.Id, Is.EqualTo(book.Id), "Main book ID should match");
        Assert.That(result.Associated, Is.Empty, "Associated books should be empty when no pattern exists");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNonExistentBookId_ReturnsEmptyResult()
    {
        // Arrange - Use a book ID that doesn't exist
        const int nonExistentBookId = 99999;

        // Act & Assert - Should throw when book doesn't exist
        Assert.That(async () => await _sut.GetOtherBooksBorrowed(nonExistentBookId),
            Throws.TypeOf<InvalidOperationException>());
    }
}
