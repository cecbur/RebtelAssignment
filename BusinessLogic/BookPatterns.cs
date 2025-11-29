using BusinessLogicContracts.Dto;
using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic;

public class BookPatterns(ILoanRepository loanRepository)
{
    private readonly ILoanRepository _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));

    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null)
    {
        var allLoans = await _loanRepository.GetAllLoans();
        var bookLoanGroups = allLoans.GroupBy(loan => loan.Book);
        var sortedBookLoanGroups = SortByLoanCountDescending(bookLoanGroups);
        var limitedBookLoanGroups = ApplyResultLimit(sortedBookLoanGroups, maxBooksToReturn);
        var bookLoansResults = MapToBookLoansDto(limitedBookLoanGroups);
        return bookLoansResults;
    }

    private static IGrouping<Book, Loan>[] SortByLoanCountDescending(
        IEnumerable<IGrouping<Book, Loan>> bookLoanGroups)
    {
        return bookLoanGroups
            .OrderByDescending(group => group.Count())
            .ToArray();
    }

    private static IGrouping<Book, Loan>[] ApplyResultLimit(
        IGrouping<Book, Loan>[] sortedBookLoanGroups,
        int? maxBooksToReturn)
    {
        if (!maxBooksToReturn.HasValue || sortedBookLoanGroups.Length <= maxBooksToReturn.Value)
            return sortedBookLoanGroups;

        return sortedBookLoanGroups
            .Take(maxBooksToReturn.Value)
            .ToArray();
    }

    private static BookLoans[] MapToBookLoansDto(IEnumerable<IGrouping<Book, Loan>> bookLoanGroups)
    {
        return bookLoanGroups
            .Select(group => new BookLoans
            {
                Book = group.Key,
                LoanCount = group.Count()
            })
            .ToArray();
    }
}
