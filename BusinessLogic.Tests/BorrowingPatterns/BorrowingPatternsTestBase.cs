using BusinessModels;
using DataStorageContracts;
using DataStorageContracts.Dto;
using Moq;

namespace BusinessLogic.Tests.BorrowingPatterns;

public abstract class BorrowingPatternsTestBase
{
    protected readonly Mock<ILoanRepository> MockLoanRepository;
    protected readonly Mock<IBorrowingPatternRepository> MockBorrowingPatternRepository;
    protected readonly BusinessLogic.BorrowingPatterns BorrowingPatterns;

    protected BorrowingPatternsTestBase()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        MockBorrowingPatternRepository = new Mock<IBorrowingPatternRepository>();
        BorrowingPatterns = new BusinessLogic.BorrowingPatterns(
            MockLoanRepository.Object,
            MockBorrowingPatternRepository.Object);
    }

    protected static Patron CreatePatron(int id, string firstName = "Test", string lastName = "Patron")
    {
        return new Patron
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
            MembershipDate = new DateTime(2023, 1, 1),
            IsActive = true
        };
    }

    protected static Book CreateBook(int id, string title, int? numberOfPages = null)
    {
        return new Book
        {
            Id = id,
            Title = title,
            NumberOfPages = numberOfPages,
            Isbn = $"978-{id:D10}",
            PublicationYear = 2020
        };
    }

    protected static Loan CreateLoan(
        int id,
        Book book,
        Patron patron,
        DateTime loanDate,
        DateTime dueDate,
        DateTime? returnDate = null,
        bool isReturned = false)
    {
        return new Loan
        {
            Id = id,
            Book = book,
            Patron = patron,
            LoanDate = loanDate,
            DueDate = dueDate,
            ReturnDate = returnDate,
            IsReturned = isReturned
        };
    }

    protected static AssociatedBooks.BookCount CreateBookCount(Book book, int count)
    {
        return new AssociatedBooks.BookCount
        {
            Book = book,
            Count = count
        };
    }

    protected static AssociatedBooks CreateAssociatedBooks(Book mainBook, params AssociatedBooks.BookCount[] associated)
    {
        return new AssociatedBooks
        {
            Book = mainBook,
            Associated = associated
        };
    }
}
