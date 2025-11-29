using BusinessLogicContracts.Dto;
using BusinessLogicContracts.Interfaces;

namespace BusinessLogic;

/// See interface for XML comments
public class Facade(BookPatterns bookPatterns, PatronActivity patronActivity, BorrowingPatterns borrowingPatterns) : IBusinessLogicFacade
{
    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null) =>
        await bookPatterns.GetBooksSortedByMostLoaned(maxBooksToReturn);

    public async Task<PatronLoans[]> GetPatronsOrderedByLoanFrequency(DateTime startDate, DateTime endDate) =>
        await patronActivity.GetPatronsOrderedByLoanFrequency(startDate, endDate);

    public async Task<double?> GetPagesPerDay(int loanId) => 
        await patronActivity.GetPagesPerDay(loanId);

    public async Task<BookFrequency[]> GetOtherBooksBorrowed(int bookId) =>
        await borrowingPatterns.GetOtherBooksBorrowed( bookId);

}