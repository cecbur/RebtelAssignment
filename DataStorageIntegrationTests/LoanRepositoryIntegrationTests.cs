using DataStorage.Repositories;
using BusinessModels;

namespace DataStorageIntegrationTests;

/// <summary>
/// Integration tests for LoanRepository using a real SQL Server database.
/// These tests use Testcontainers to spin up a SQL Server instance in Docker.
/// Tests in this fixture share database state and must NOT run in parallel.
/// </summary>
public class LoanRepositoryIntegrationTests : RepositoryIntegrationTestBase<LoanRepository>
{
    protected override LoanRepository CreateRepository()
    {
        return new LoanRepository(_connectionFactory);
    }

    [Test]
    public async Task GetAllLoans_WithMultipleLoans_ReturnsAllLoansWithJoinedData()
    {
        // Arrange - Create test data
        var author1 = await _testData.CreateAuthor("George", "Orwell");
        var author2 = await _testData.CreateAuthor("Aldous", "Huxley");

        var book1 = await _testData.CreateBook("1984", author1.Id, "978-0451524935");
        var book2 = await _testData.CreateBook("Brave New World", author2.Id, "978-0060850524");

        var patron1 = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Smith", "bob@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        var loan1 = await _testData.CreateLoan(book1.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true);
        var loan2 = await _testData.CreateLoan(book2.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), null, false);
        var loan3 = await _testData.CreateLoan(book1.Id, patron2.Id, baseDate.AddDays(10), baseDate.AddDays(24), baseDate.AddDays(20), true);

        // Act
        var result = await _sut.GetAllLoans();

        // Assert
        var loans = result.ToArray();
        
        Assert.That(loans, Has.Length.EqualTo(3), "Should return all 3 loans");

        // Verify loans are ordered by LoanDate DESC (most recent first)
        Assert.That(loans[0].LoanDate, Is.EqualTo(loan3.LoanDate));
        Assert.That(loans[1].LoanDate, Is.EqualTo(loan2.LoanDate));
        Assert.That(loans[2].LoanDate, Is.EqualTo(loan1.LoanDate));

        // Verify first loan has all joined data (loan3 was for book1 and patron2)
        var firstLoan = loans[0];
        Assert.That(firstLoan.Book, Is.Not.Null, "Book should be joined");
        Assert.That(firstLoan.Book.Title, Is.EqualTo(book1.Title));
        Assert.That(firstLoan.Patron, Is.Not.Null, "Patron should be joined");
        Assert.That(firstLoan.Patron.FirstName, Is.EqualTo(patron2.FirstName));
        Assert.That(firstLoan.IsReturned, Is.True);
        Assert.That(firstLoan.ReturnDate, Is.EqualTo(loan3.ReturnDate));
    }

    [Test]
    public async Task GetAllLoans_WithNoLoans_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllLoans();

        // Assert
        Assert.That(result, Is.Empty, "Should return empty collection when no loans exist");
    }

    [Test]
    public async Task GetLoanById_WithValidId_ReturnsLoanWithAllJoinedData()
    {
        // Arrange
        var author = await _testData.CreateAuthor("George", "Orwell");
        var book = await _testData.CreateBook("1984", author.Id, "978-0451524935", 1949, 328);
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com", "555-1234");

        var baseDate = new DateTime(2024, 1, 15);
        var createdLoan = await _testData.CreateLoan(
            book.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true);

        // Act
        var result = await _sut.GetLoanById(createdLoan.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdLoan.Id));
        Assert.That(result.LoanDate, Is.EqualTo(createdLoan.LoanDate));
        Assert.That(result.DueDate, Is.EqualTo(createdLoan.DueDate));
        Assert.That(result.ReturnDate, Is.EqualTo(createdLoan.ReturnDate));
        Assert.That(result.IsReturned, Is.True);

        // Verify Book is populated
        Assert.That(result.Book, Is.Not.Null);
        Assert.That(result.Book.Title, Is.EqualTo(book.Title));
        Assert.That(result.Book.NumberOfPages, Is.EqualTo(book.NumberOfPages));
        Assert.That(result.Book.Isbn, Is.EqualTo(book.Isbn));

        // Verify Patron is populated
        Assert.That(result.Patron, Is.Not.Null);
        Assert.That(result.Patron.FirstName, Is.EqualTo(patron.FirstName));
        Assert.That(result.Patron.LastName, Is.EqualTo(patron.LastName));
        Assert.That(result.Patron.Email, Is.EqualTo(patron.Email));
    }

    [Test]
    public async Task GetLoanById_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        const int nonExistentId = 99999;

        // Act & Assert
        Assert.That(async () => await _sut.GetLoanById(nonExistentId),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains($"Loan with id {nonExistentId} not found"));
    }

    [Test]
    public async Task GetLoansByPatronId_WithMultipleLoans_ReturnsOnlyPatronLoans()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book1 = await _testData.CreateBook("Book 1", author.Id, "978-1111111111");
        var book2 = await _testData.CreateBook("Book 2", author.Id, "978-2222222222");

        var patron1 = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Smith", "bob@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        var loan1 = await _testData.CreateLoan(book1.Id, patron1.Id, baseDate, baseDate.AddDays(14), null, false);
        var loan2 = await _testData.CreateLoan(book2.Id, patron1.Id, baseDate.AddDays(5), baseDate.AddDays(19), null, false);
        var loan3 = await _testData.CreateLoan(book1.Id, patron2.Id, baseDate.AddDays(10), baseDate.AddDays(24), null, false);

        // Act
        var result = await _sut.GetLoansByPatronId(patron1.Id);
        var loans = result.ToArray();

        // Assert
        Assert.That(loans, Has.Length.EqualTo(2), "Should return only patron1's loans");
        Assert.That(loans.All(l => l.Patron.Id == patron1.Id), Is.True, "All loans should belong to patron1");

        // Verify ordering by LoanDate DESC
        Assert.That(loans[0].LoanDate, Is.EqualTo(loan2.LoanDate));
        Assert.That(loans[1].LoanDate, Is.EqualTo(loan1.LoanDate));
    }

    [Test]
    public async Task GetLoansByPatronId_WithNoLoans_ReturnsEmptyCollection()
    {
        // Arrange
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        // Act
        var result = await _sut.GetLoansByPatronId(patron.Id);

        // Assert
        Assert.That(result, Is.Empty, "Should return empty collection for patron with no loans");
    }

    [Test]
    public async Task GetLoansByBookId_WithMultipleLoans_ReturnsOnlyBookLoans()
    {
        // Arrange
        var author = await _testData.CreateAuthor("George", "Orwell");
        var book1 = await _testData.CreateBook("1984", author.Id, "978-0451524935");
        var book2 = await _testData.CreateBook("Animal Farm", author.Id, "978-0451526342");

        var patron1 = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Smith", "bob@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        var loan1 = await _testData.CreateLoan(book1.Id, patron1.Id, baseDate, baseDate.AddDays(14), null, false);
        var loan2 = await _testData.CreateLoan(book1.Id, patron2.Id, baseDate.AddDays(5), baseDate.AddDays(19), null, false);
        var loan3 = await _testData.CreateLoan(book2.Id, patron1.Id, baseDate.AddDays(10), baseDate.AddDays(24), null, false);

        // Act
        var result = await _sut.GetLoansByBookId(book1.Id);
        var loans = result.ToArray();

        // Assert
        Assert.That(loans, Has.Length.EqualTo(2), "Should return only book1's loans");
        Assert.That(loans.All(l => l.Book.Id == book1.Id), Is.True, "All loans should be for book1");

        // Verify books have titles (both loans are for book1)
        Assert.That(loans[0].Book.Title, Is.EqualTo(book1.Title));
        Assert.That(loans[1].Book.Title, Is.EqualTo(book1.Title));
    }

    [Test]
    public async Task GetActiveLoans_WithMixedLoans_ReturnsOnlyActiveLoans()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book1 = await _testData.CreateBook("Book 1", author.Id, "978-3333333333");
        var book2 = await _testData.CreateBook("Book 2", author.Id, "978-4444444444");
        var book3 = await _testData.CreateBook("Book 3", author.Id, "978-5555555555");

        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        // Active loan (not returned)
        var loan1 = await _testData.CreateLoan(book1.Id, patron.Id, baseDate, baseDate.AddDays(14), null, false);
        // Returned loan
        var loan2 = await _testData.CreateLoan(book2.Id, patron.Id, baseDate.AddDays(5), baseDate.AddDays(19), baseDate.AddDays(15), true);
        // Another active loan
        var loan3 = await _testData.CreateLoan(book3.Id, patron.Id, baseDate.AddDays(10), baseDate.AddDays(24), null, false);

        // Act
        var result = await _sut.GetActiveLoans();
        var loans = result.ToArray();

        // Assert
        Assert.That(loans, Has.Length.EqualTo(2), "Should return only active (not returned) loans");
        Assert.That(loans.All(l => !l.IsReturned), Is.True, "All returned loans should have IsReturned=false");

        // Verify ordering by DueDate ASC (loan1 due date < loan3 due date)
        Assert.That(loans[0].DueDate, Is.EqualTo(loan1.DueDate));
        Assert.That(loans[1].DueDate, Is.EqualTo(loan3.DueDate));
        Assert.That(loans[0].DueDate, Is.LessThan(loans[1].DueDate), "Should be ordered by DueDate ascending");
    }

    [Test]
    public async Task GetActiveLoans_WithNoActiveLoans_ReturnsEmptyCollection()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Book 1", author.Id, "978-6666666666");
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var baseDate = new DateTime(2024, 1, 1);
        // Create only returned loans
        var loan = await _testData.CreateLoan(book.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true);

        // Assert loan was created with returned status
        Assert.That(loan.IsReturned, Is.True, "Could not create test data! Created loan should be marked as returned");
        Assert.That(loan.ReturnDate, Is.EqualTo(baseDate.AddDays(10)), "Could not create test data! Return date should match");

        // Act
        var result = await _sut.GetActiveLoans();

        // Assert
        Assert.That(result, Is.Empty, "Should return empty collection when all loans are returned");
    }

    [Test]
    public async Task AddLoan_WithValidData_CreatesLoanAndReturnsCompleteObject()
    {
        // Arrange
        var author = await _testData.CreateAuthor("George", "Orwell");
        var book = await _testData.CreateBook("1984", author.Id, "978-0451524935");
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var baseDate = new DateTime(2024, 3, 15);
        var newLoan = new Loan
        {
            Book = book,
            Patron = patron,
            LoanDate = baseDate,
            DueDate = baseDate.AddDays(14),
            ReturnDate = null,
            IsReturned = false
        };

        // Act
        var result = await _sut.AddLoan(newLoan);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0), "Should have generated ID");
        Assert.That(result.LoanDate, Is.EqualTo(newLoan.LoanDate));
        Assert.That(result.DueDate, Is.EqualTo(newLoan.DueDate));
        Assert.That(result.ReturnDate, Is.Null);
        Assert.That(result.IsReturned, Is.False);

        // Verify joined data is populated
        Assert.That(result.Book, Is.Not.Null);
        Assert.That(result.Book.Title, Is.EqualTo(book.Title));
        Assert.That(result.Patron, Is.Not.Null);
        Assert.That(result.Patron.Email, Is.EqualTo(patron.Email));

        // Verify it's actually in the database
        var retrievedLoan = await _sut.GetLoanById(result.Id);
        Assert.That(retrievedLoan.LoanDate, Is.EqualTo(newLoan.LoanDate));
    }

    [Test]
    public async Task AddLoan_WithNullLoan_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.That(async () => await _sut.AddLoan(null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public async Task UpdateLoan_WithValidData_UpdatesLoanSuccessfully()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-7777777777");
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var baseDate = new DateTime(2024, 2, 1);
        var createdLoan = await _testData.CreateLoan(
            book.Id, patron.Id, baseDate, baseDate.AddDays(14), null, false);

        // Create updated loan object
        var updatedLoan = new Loan
        {
            Id = createdLoan.Id,
            Book = book,
            Patron = patron,
            LoanDate = createdLoan.LoanDate,
            DueDate = createdLoan.DueDate,
            ReturnDate = baseDate.AddDays(10),
            IsReturned = true
        };

        // Act
        var result = await _sut.UpdateLoan(updatedLoan);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdLoan.Id));
        Assert.That(result.ReturnDate, Is.EqualTo(baseDate.AddDays(10)));
        Assert.That(result.IsReturned, Is.True);

        // Verify it's actually updated in the database
        var retrievedLoan = await _sut.GetLoanById(createdLoan.Id);
        Assert.That(retrievedLoan.IsReturned, Is.True);
        Assert.That(retrievedLoan.ReturnDate, Is.EqualTo(baseDate.AddDays(10)));
    }

    [Test]
    public async Task UpdateLoan_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-8888888888");
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var nonExistentLoan = new Loan
        {
            Id = 99999,
            Book = book,
            Patron = patron,
            LoanDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(14),
            IsReturned = false
        };

        // Act & Assert
        Assert.That(async () => await _sut.UpdateLoan(nonExistentLoan),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains("Loan with id 99999 not found"));
    }

    [Test]
    public async Task DeleteLoan_WithValidId_DeletesLoanAndReturnsTrue()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-9999999999");
        var patron = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");

        var baseDate = DateTime.Now;
        var createdLoan = await _testData.CreateLoan(
            book.Id, patron.Id, baseDate, baseDate.AddDays(14), null, false);

        // Act
        var result = await _sut.DeleteLoan(createdLoan.Id);

        // Assert
        Assert.That(result, Is.True, "Should return true when loan is deleted");

        // Verify it's actually deleted
        Assert.That(async () => await _sut.GetLoanById(createdLoan.Id),
            Throws.TypeOf<InvalidOperationException>(),
            "Should throw when trying to get deleted loan");
    }

    [Test]
    public async Task DeleteLoan_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        const int nonExistentId = 99999;

        // Act
        var result = await _sut.DeleteLoan(nonExistentId);

        // Assert
        Assert.That(result, Is.False, "Should return false when loan doesn't exist");
    }
}
