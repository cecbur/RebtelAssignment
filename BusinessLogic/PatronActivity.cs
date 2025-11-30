using BusinessLogicContracts.Dto;
using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic;

public class PatronActivity(ILoanRepository loanRepository)
{
    private readonly ILoanRepository _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));

    public async Task<PatronLoans[]> GetPatronsOrderedByLoanFrequency(
        DateTime startDate,
        DateTime endDate)
    {
        var loans = await _loanRepository.GetLoansByTime(startDate, endDate);
        var patronLoans = GroupLoansByPatron(loans);
        var sortedPatronLoans = SortPatronsByLoanCount(patronLoans);
        return sortedPatronLoans;
    }

    public async Task<double?> GetPagesPerDay(int loanId)
    {
        var loan = await _loanRepository.GetLoanById(loanId);

        if (loan.ReturnDate is null || loan.Book.NumberOfPages is null)
            return null;

        var readingPace = CalculateReadingPace(loan);
        return readingPace;
    }

    private static PatronLoans[] GroupLoansByPatron(IEnumerable<Loan> loans)
    {
        var patronLoans = loans
            .GroupBy(loan => loan.Patron)
            .Select(group => new PatronLoans(group.Key, group.ToArray()))
            .ToArray();
        return patronLoans;
    }

    private static PatronLoans[] SortPatronsByLoanCount(
        PatronLoans[] patronLoans)
    {
        var sortedPatronLoans = patronLoans
            .OrderByDescending(patron => patron.LoanCount)
            .ToArray();
        return sortedPatronLoans;
    }

    private static double CalculateReadingPace(Loan loan)
    {
        var loanDurationInDays = (loan.ReturnDate - loan.LoanDate)!.Value.TotalDays;
        var pagesPerDay = loan.Book.NumberOfPages!.Value / loanDurationInDays;
        return pagesPerDay;
    }
}
