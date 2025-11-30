using BusinessModels;
using Moq;

namespace BusinessLogicTests.BookPatterns;

public class GetBooksSortedByMostLoanedTests : BookPatternsTestBase
{
    [Test]
    public async Task GetBooksSortedByMostLoaned_WithMultipleBooks_ReturnsSortedByLoanCount()
    {
        // Arrange
        var popularBook = CreateBook(1, "Popular Book");
        var veryPopularBook = CreateBook(2, "Very Popular Book");
        var lessPopularBook = CreateBook(3, "Less Popular Book");

        var patron = CreatePatron(1, "Test", "Patron");

        var loans = new List<Loan>
        {
            CreateLoan(1, popularBook, patron, DateTime.Now.AddDays(-10), DateTime.Now),
            CreateLoan(2, popularBook, patron, DateTime.Now.AddDays(-20), DateTime.Now),
            CreateLoan(3, veryPopularBook, patron, DateTime.Now.AddDays(-15), DateTime.Now),
            CreateLoan(4, veryPopularBook, patron, DateTime.Now.AddDays(-25), DateTime.Now),
            CreateLoan(5, veryPopularBook, patron, DateTime.Now.AddDays(-30), DateTime.Now),
            CreateLoan(6, lessPopularBook, patron, DateTime.Now.AddDays(-5), DateTime.Now)
        };

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { popularBook, veryPopularBook, lessPopularBook });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned();

        // Assert
        Assert.That(result.Length, Is.EqualTo(3), "Should return all 3 books");
        Assert.That(result[0].Book.Id, Is.EqualTo(veryPopularBook.Id), "Most loaned book should be first");
        Assert.That(result[0].LoanCount, Is.EqualTo(3), "Most loaned book should have 3 loans");
        Assert.That(result[1].Book.Id, Is.EqualTo(popularBook.Id), "Second most loaned book should be second");
        Assert.That(result[1].LoanCount, Is.EqualTo(2), "Second most loaned book should have 2 loans");
        Assert.That(result[2].Book.Id, Is.EqualTo(lessPopularBook.Id), "Least loaned book should be last");
        Assert.That(result[2].LoanCount, Is.EqualTo(1), "Least loaned book should have 1 loan");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithNoLoans_ReturnsEmptyArray()
    {
        // Arrange
        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new List<Book>());

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned();

        // Assert
        Assert.That(result.Length, Is.EqualTo(0), "Should return empty array when there are no loans");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithMaxBooksLimit_ReturnsLimitedResults()
    {
        // Arrange
        var book1 = CreateBook(1, "Book 1");
        var book2 = CreateBook(2, "Book 2");
        var book3 = CreateBook(3, "Book 3");
        var book4 = CreateBook(4, "Book 4");

        var patron = CreatePatron(1, "Test", "Patron");

        var loans = new List<Loan>
        {
            CreateLoan(1, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(3, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(4, book2, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(5, book2, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(6, book3, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(7, book4, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2, book3, book4 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned(maxBooksToReturn: 2);

        // Assert
        Assert.That(result.Length, Is.EqualTo(2), "Should return only 2 books when max is set to 2");
        Assert.That(result[0].Book.Id, Is.EqualTo(book1.Id), "First book should be the most loaned");
        Assert.That(result[0].LoanCount, Is.EqualTo(3), "First book should have 3 loans");
        Assert.That(result[1].Book.Id, Is.EqualTo(book2.Id), "Second book should be the second most loaned");
        Assert.That(result[1].LoanCount, Is.EqualTo(2), "Second book should have 2 loans");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithMaxBooksGreaterThanAvailable_ReturnsAllBooks()
    {
        // Arrange
        var book1 = CreateBook(1, "Book 1");
        var book2 = CreateBook(2, "Book 2");

        var patron = CreatePatron(1, "Test", "Patron");

        var loans = new List<Loan>
        {
            CreateLoan(1, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, book2, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned(maxBooksToReturn: 10);

        // Assert
        Assert.That(result.Length, Is.EqualTo(2), "Should return all available books when max exceeds available count");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithSingleBook_ReturnsSingleBookLoanCount()
    {
        // Arrange
        var book = CreateBook(1, "Only Book");
        var patron = CreatePatron(1, "Test", "Patron");

        var loans = new List<Loan>
        {
            CreateLoan(1, book, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, book, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(3, book, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned();

        // Assert
        Assert.That(result.Length, Is.EqualTo(1), "Should return exactly one book");
        Assert.That(result[0].Book.Id, Is.EqualTo(book.Id), "Returned book should match the only book");
        Assert.That(result[0].LoanCount, Is.EqualTo(3), "Book should have correct loan count of 3");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithEqualLoanCounts_MaintainsOrder()
    {
        // Arrange
        var book1 = CreateBook(1, "Book A");
        var book2 = CreateBook(2, "Book B");
        var book3 = CreateBook(3, "Book C");

        var patron = CreatePatron(1, "Test", "Patron");

        var loans = new List<Loan>
        {
            CreateLoan(1, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, book1, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(3, book2, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(4, book2, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(5, book3, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(6, book3, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new[] { book1, book2, book3 });

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await BookPatterns.GetBooksSortedByMostLoaned();

        // Assert
        Assert.That(result.Length, Is.EqualTo(3), "Should return all 3 books");
        Assert.That(result[0].LoanCount, Is.EqualTo(2), "All books should have equal loan count of 2");
        Assert.That(result[1].LoanCount, Is.EqualTo(2), "All books should have equal loan count of 2");
        Assert.That(result[2].LoanCount, Is.EqualTo(2), "All books should have equal loan count of 2");
    }

    [Test]
    public void GetBooksSortedByMostLoaned_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        MockBookRepository
            .Setup(r => r.GetAllBooks())
            .ReturnsAsync(new List<Book>());

        MockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await BookPatterns.GetBooksSortedByMostLoaned());
    }

}
