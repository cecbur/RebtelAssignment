using BusinessModels;
using Moq;

namespace BusinessLogicTests.BorrowingPatterns;

public class GetOtherBooksBorrowedTests : BorrowingPatternsTestBase
{
    [Test]
    public async Task GetOtherBooksBorrowed_WithMultipleAssociatedBooks_ReturnsSortedByFrequency()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var associatedBook1 = CreateBook(2, "Associated Book 1");
        var associatedBook2 = CreateBook(3, "Associated Book 2");
        var associatedBook3 = CreateBook(4, "Associated Book 3");

        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(3, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(4, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(5, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(
            mainBook,
            CreateBookCount(associatedBook1, 4),
            CreateBookCount(associatedBook2, 3),
            CreateBookCount(associatedBook3, 2)
        );

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(3), "Should return all 3 associated books");

        Assert.That(result[0].AssociatedBook.Id, Is.EqualTo(associatedBook1.Id), "Most frequently borrowed book should be first");
        Assert.That(result[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.8).Within(0.01), "First book should have ratio of 0.8 (4/5)");

        Assert.That(result[1].AssociatedBook.Id, Is.EqualTo(associatedBook2.Id), "Second most frequently borrowed book should be second");
        Assert.That(result[1].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.6).Within(0.01), "Second book should have ratio of 0.6 (3/5)");

        Assert.That(result[2].AssociatedBook.Id, Is.EqualTo(associatedBook3.Id), "Third most frequently borrowed book should be third");
        Assert.That(result[2].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(0.4).Within(0.01), "Third book should have ratio of 0.4 (2/5)");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithBooksWithOnlyOneLoan_FiltersThemOut()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var frequentlyBorrowedAssociatedBook = CreateBook(2, "Frequently Borrowed");
        var onceBorrowedAssociatedBook = CreateBook(3, "Once Borrowed");

        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(
            mainBook,
            CreateBookCount(frequentlyBorrowedAssociatedBook, 3),
            CreateBookCount(onceBorrowedAssociatedBook, 1)
        );

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(1), "Should return only one book after filtering out books with single loan");
        Assert.That(result[0].AssociatedBook.Id, Is.EqualTo(frequentlyBorrowedAssociatedBook.Id), "Frequently borrowed book should be in result");
        Assert.That(result[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(1.5).Within(0.01), "Book should have ratio of 1.5 (3/2)");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithNoAssociatedBooks_ReturnsEmptyArray()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(mainBook);

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(0), "Should return empty array when there are no associated books");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithAllBooksHavingOneLoan_ReturnsEmptyArray()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var associatedBook1 = CreateBook(2, "Book 1");
        var associatedBook2 = CreateBook(3, "Book 2");

        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(
            mainBook,
            CreateBookCount(associatedBook1, 1),
            CreateBookCount(associatedBook2, 1)
        );

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(0), "Should return empty array when all associated books have only one loan");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithEqualFrequencies_MaintainsOrder()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var associatedBook1 = CreateBook(2, "Book A");
        var associatedBook2 = CreateBook(3, "Book B");
        var associatedBook3 = CreateBook(4, "Book C");

        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(
            mainBook,
            CreateBookCount(associatedBook1, 3),
            CreateBookCount(associatedBook2, 3),
            CreateBookCount(associatedBook3, 3)
        );

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(3), "Should return all 3 books with equal loan counts");
        Assert.That(result[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(1.5).Within(0.01), "First book should have ratio of 1.5");
        Assert.That(result[1].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(1.5).Within(0.01), "Second book should have ratio of 1.5");
        Assert.That(result[2].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(1.5).Within(0.01), "Third book should have ratio of 1.5");
    }

    [Test]
    public async Task GetOtherBooksBorrowed_WithHigherFrequencyThanMainBook_CalculatesCorrectRatio()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var associatedBook = CreateBook(2, "Very Popular Book");

        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14)),
            CreateLoan(2, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        var associatedBooks = CreateAssociatedBooks(
            mainBook,
            CreateBookCount(associatedBook, 10)
        );

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(1))
            .ReturnsAsync(associatedBooks);

        // Act
        var result = await BorrowingPatterns.GetOtherBooksBorrowed(1);

        // Assert
        Assert.That(result.Length, Is.EqualTo(1), "Should return the associated book with higher frequency");
        Assert.That(result[0].LoansOfThisBookPerLoansOfMainBook, Is.EqualTo(5.0).Within(0.01), "Book should have ratio of 5.0 (10/2)");
    }

    [Test]
    public void GetOtherBooksBorrowed_WhenLoanRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await BorrowingPatterns.GetOtherBooksBorrowed(1));
    }

    [Test]
    public void GetOtherBooksBorrowed_WhenBorrowingPatternRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var mainBook = CreateBook(1, "Main Book");
        var patron = CreatePatron(1, "Test", "Patron");

        var mainBookLoans = new List<Loan>
        {
            CreateLoan(1, mainBook, patron, DateTime.Now, DateTime.Now.AddDays(14))
        };

        MockLoanRepository
            .Setup(r => r.GetLoansByBookId(1))
            .ReturnsAsync(mainBookLoans);

        MockBorrowingPatternRepository
            .Setup(r => r.GetOtherBooksBorrowed(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Repository failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await BorrowingPatterns.GetOtherBooksBorrowed(1));
    }
}
