using BusinessLogicContracts.Dto;
using DataStorageContracts;
using DataStorageContracts.Dto;

namespace BusinessLogic;

public class BorrowingPatterns(ILoanRepository loanRepository, IBorrowingPatternRepository borrowingPatternRepository)
{
    private readonly ILoanRepository _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
    private readonly IBorrowingPatternRepository _borrowingPatternRepository = borrowingPatternRepository ?? throw new ArgumentNullException(nameof(borrowingPatternRepository));

    public async Task<BookFrequency[]> GetOtherBooksBorrowed(int bookId)
    {
        var mainBookLoans = await _loanRepository.GetLoansByBookId(bookId);
        var mainBookLoanCount = mainBookLoans.Count();
        var associatedBooksData = await _borrowingPatternRepository.GetOtherBooksBorrowed(bookId);
        var significantBooks = FilterSignificantBooks(associatedBooksData);
        var sortedBooks = SortByFrequency(significantBooks);
        var bookFrequencies = MapToBookFrequencies(sortedBooks, mainBookLoanCount);
        return bookFrequencies;
    }

    private static AssociatedBooks.BookCount[] FilterSignificantBooks(
        AssociatedBooks associatedBooksData)
    {
        var filteredBooks = associatedBooksData.Associated
            .Where(bookData => bookData.Count > 1)
            .ToArray();
        return filteredBooks;
    }

    private static AssociatedBooks.BookCount[] SortByFrequency(
        AssociatedBooks.BookCount[] books)
    {
        var sortedBooks = books
            .OrderByDescending(bookData => bookData.Count)
            .ToArray();
        return sortedBooks;
    }

    private static BookFrequency[] MapToBookFrequencies(
        AssociatedBooks.BookCount[] sortedBooks,
        int mainBookLoanCount)
    {
        var bookFrequencies = sortedBooks
            .Select(bookData => new BookFrequency
            {
                AssociatedBook = bookData.Book,
                LoansOfThisBookPerLoansOfMainBook =
                    (double)bookData.Count / mainBookLoanCount
            })
            .ToArray();
        return bookFrequencies;
    }
}
