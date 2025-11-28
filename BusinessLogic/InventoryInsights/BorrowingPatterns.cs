using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic.InventoryInsights;

public class BorrowingPatterns(ILoanRepository loanRepository, IBorrowingPatternRepository borrowingPatternRepository)
{
    public async Task<BookFrequency[]> GetPatronLoansOrderedByFrequency(int bookId)
    {
        var loans = await loanRepository.GetLoansByBookId(bookId);
        var loanCount = loans.Count();
        var associatedBooks = await borrowingPatternRepository.GetOtherBooksBorrowed(bookId);
        var bookFrequencies = associatedBooks.Associated
            .Where(x => x.Count > 1)
            .OrderByDescending(x => x.Count)
            .Select(x => new BookFrequency()
            {
                AssociatedBook = x.Book,
                LoansOfThisBookPerLoansOfMainBook = (double)x.Count / loanCount

            })
            .ToArray();
        return bookFrequencies;
    }

    public class BookFrequency
    {
        public required Book AssociatedBook { get; set; }
        public double LoansOfThisBookPerLoansOfMainBook { get; set; }
    }

}