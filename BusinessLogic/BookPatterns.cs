using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic;

public class BookPatterns(ILoanRepository loanRepository)
{
    public async Task<BookLoans[]> GetMostBorrowedBooksSorted()
    {
        var loans = await loanRepository.GetAllLoans();
        var bookLoans = loans
            .GroupBy(l => l.Book)
            .OrderByDescending(x => x.Count())
            .Select(x => new BookLoans()
            {
                Book = x.Key,
                Loans = x.ToArray(),
            })
            .ToArray();
        return bookLoans;
    }
    
    public class BookLoans
    {
        public required Book Book { get; set; }
        public required Loan[] Loans { get; set; }
        
        public int LoanCount => Loans.Length;
    }
    
}