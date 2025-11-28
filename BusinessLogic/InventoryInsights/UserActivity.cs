
using BusinessModels;
using DataStorageClient;

namespace BusinessLogic.InventoryInsights;

public class UserActivity(LoanRepository loanRepository)
{
    private readonly LoanRepository _loanRepository = loanRepository;

    public async Task<PatronLoans[]> PatronLoansOrderedByFrequency(DateTime startDate, DateTime endDate)
    {
        var loans = await _loanRepository.GetAllLoans();
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
