using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic;

public class BookPatterns(ILoanRepository loanRepository)
{
    public async Task<BookLoans[]> GetMostLoanedBooksSorted(int? maxBooksToReturn = null)
    {
        var loans = await loanRepository.GetAllLoans();
        var loansSorted = loans
            .GroupBy(l => l.Book)
            .OrderByDescending(x => x.Count())
            .ToArray();
        
        if (maxBooksToReturn != null && loansSorted.Length > maxBooksToReturn)
            loansSorted = loansSorted.Take(maxBooksToReturn.Value).ToArray();
        
        var bookLoans = loansSorted
            .Select(x => new BookLoans()
            {
                Book = x.Key,
                LoanCount = x.Count(),
            })
            .ToArray();
        return bookLoans;
    }
    
    public class BookLoans
    {
        public required Book Book { get; set; }
        public int LoanCount { get; set; }
    }
    
}