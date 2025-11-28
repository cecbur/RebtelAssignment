
using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic.InventoryInsights;

public class UserActivity(ILoanRepository loanRepository)
{
    public async Task<PatronLoans[]> GetPatronLoansOrderedByFrequency(DateTime startDate, DateTime endDate)
    {
        IEnumerable<Loan> loans = await loanRepository.GetLoansByTime(startDate, endDate);
        var patronLoans = GetPatronLoans(loans);
        return patronLoans.OrderByDescending(x => x.LoanCount).ToArray();
    }

    /// <summary>
    /// Users estimated reading pace assuming continuous reading
    /// </summary>
    /// <param name="loanId">ID of the loan</param>
    /// <returns>Number of pages read per day. Null if the book is not yet returned</returns>
    public async Task<double?> GetPagesPerDay(int loanId)
    {
        var loan = await loanRepository.GetLoanById(loanId);
        if (loan.ReturnDate is null)
            return null;
        var time = (loan.ReturnDate - loan.LoanDate).Value.TotalDays;
        var pace = loan.Book.NumberOfPages / time;
        return pace;
    }
    
    private PatronLoans[] GetPatronLoans(IEnumerable<Loan> loans)
    {
        var patronLoans = loans
            .GroupBy(l => l.Patron)
            .Select(x => new PatronLoans(x.Key, x.ToArray()))
            .ToArray();
        return patronLoans;
    }

    
    public class PatronLoans
    {
        public PatronLoans(Patron patron, Loan[] loans)
        {
            Patron = patron;
            Loans = loans;
        }

        public int LoanCount => Loans.Count();
        public Patron Patron { get;}
        public Loan[] Loans { get;}
    }
    
}
