using BusinessLogicContracts.Dto;

namespace BusinessLogic;

public class Facade(BookPatterns bookPatterns, PatronActivity patronActivity, BorrowingPatterns borrowingPatterns) : IBusinessLogicFacade
{

    /// <summary>
    /// 1. Inventory Insights: What are the most borrowed books? 
    /// Gets all books sorted by how many times they were loaned (most loaned first)
    /// </summary>
    /// <param name="maxBooks">Optional maximum number of books to return</param>
    /// <returns>List of books with their loan counts, ordered by loan count descending</returns>
    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null) =>
        await bookPatterns.GetBooksSortedByMostLoaned(maxBooksToReturn);

    /// <summary>
    /// 2. User Activity: Which users have borrowed the most books within a given time frame?
    /// Gets patrons ordered by loan frequency within a given time frame
    /// </summary>
    /// <param name="startDate">Start date of the time frame</param>
    /// <param name="endDate">End date of the time frame</param>
    /// <param name="maxPatrons">The maximum number of patrons to return</param>
    /// <returns>List of patrons ordered by loan count (descending)</returns>
    public async Task<PatronLoans[]> GetPatronsOrderedByLoanFrequency(DateTime startDate, DateTime endDate) =>
        await patronActivity.GetPatronsOrderedByLoanFrequency(startDate, endDate);

    /// <summary>
    /// 3. User Activity: Estimate a users reading pace (pages per day)
    ///    based on the borrow and return duration of a book, assuming continuous reading.
    /// Gets the average reading pace (pages per day) for a specific loan
    /// </summary>
    /// <param name="loanId">The ID of the loan</param>
    /// <returns>The patron's reading pace in pages per day. Null if the book is not yet returned</returns>
    public async Task<double?> GetPagesPerDay(int loanId) => 
        await patronActivity.GetPagesPerDay(loanId);

    /// <summary>
    /// 4. Borrowing patterns: What other books were borrowed by individuals who borrowed a specific book?
    /// Gets other books that were borrowed by individuals who borrowed a specific book,
    /// filtered to books borrowed more than once and ordered by frequency by loan ratios.
    /// I.e. how much more often was the other book borrowed compared to the specific book
    /// </summary>
    /// <param name="bookId">The ID of the book to analyze</param>
    /// <returns>Associated books with frequency ratios, ordered by frequency descending</returns>
    public async Task<BookFrequency[]> GetOtherBooksBorrowed(int bookId) =>
        await borrowingPatterns.GetOtherBooksBorrowed( bookId);

}