using BusinessModels;
using DataStorageContracts;
using DataStorageContracts.Dto;
using Moq;

namespace BusinessLogicTests.BorrowingPatterns;

public abstract class BorrowingPatternsTestBase : CommonTestBase
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
