
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

    public async Task<Dictionary<Patron, double?>> GetPagesPerDayByPatron()
    {
        var loans = await loanRepository.GetAllLoans();
        var patronLoans = GetPatronLoans(loans);
        var readingPace = new Dictionary<Patron, double?>();
        foreach (var patronLoan in patronLoans)
        {
            var pagesPerDay = CalculatePagesPerDay(patronLoan.Loans);
            if (pagesPerDay.HasValue)
            {
                readingPace.Add(patronLoan.Patron, pagesPerDay.Value);
            }
        }

        return readingPace;
    }

    public async Task<double?> GetPagesPerDayByPatron(int patronId)
    {
        var loans = await loanRepository.GetLoansByPatronId(patronId);
        return CalculatePagesPerDay(loans);
    }

    private double? CalculatePagesPerDay(IEnumerable<Loan> loans)
    {
        var returnedLoans = loans
            .Where(l => l.ReturnDate is not null && l.Book.NumberOfPages is not null)
            .ToArray();

        if (returnedLoans.Length == 0)
            return null;

        var loanTimes = returnedLoans
            .Select(l => (l.LoanDate, (DateTime)l.ReturnDate!))
            .ToArray();
        var time = GetTotalLoanTime(loanTimes);
        
        var pages = returnedLoans.Sum(l => l.Book.NumberOfPages!)!.Value;
        var pagesPerDay = pages / time.TotalDays;

        return pagesPerDay;
    }

    private PatronLoans[] GetPatronLoans(IEnumerable<Loan> loans)
    {
        var patronLoans = loans
            .GroupBy(l => l.Patron)
            .Select(x => new PatronLoans(x.Key, x.ToArray()))
            .ToArray();
        return patronLoans;
    }

    private TimeSpan GetTotalLoanTime(IEnumerable<(DateTime Start, DateTime End)> loanTimes)
    {
        var ordered = loanTimes
            .Where(i => i.Start < i.End)
            .OrderBy(i => i.Start)
            .ToList();

        if (ordered.Count == 0)
            return TimeSpan.Zero;

        // Merge
        var merged = new List<(DateTime Start, DateTime End)>();
        var currentStart = ordered[0].Start;
        var currentEnd = ordered[0].End;

        for (int i = 1; i < ordered.Count; i++)
        {
            var (s, e) = ordered[i];
            if (s <= currentEnd) // overlap or touch
            {
                if (e > currentEnd)
                    currentEnd = e;
            }
            else
            {
                merged.Add((currentStart, currentEnd));
                currentStart = s;
                currentEnd = e;
            }
        }

        merged.Add((currentStart, currentEnd));

        // Sum lengths
        return merged
            .Select(m => m.End - m.Start)
            .Aggregate(TimeSpan.Zero, (acc, t) => acc + t);
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
