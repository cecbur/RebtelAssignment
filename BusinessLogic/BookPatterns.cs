using BusinessLogicContracts.Dto;
using BusinessModels;
using DataStorageContracts;

namespace BusinessLogic;

public class BookPatterns(ILoanRepository loanRepository, IBookRepository bookRepository)
{
    private readonly ILoanRepository _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));

    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null)
    {
        var allBooks = await _bookRepository.GetAllBooks();
        var allLoans = await _loanRepository.GetAllLoans();

        var bookLoansQuery = allBooks
            .Select(book => new BookLoans
            {
                Book = book,
                LoanCount = allLoans.Count(loan => loan.Book.Id == book.Id)
            })
            .OrderByDescending(x => x.LoanCount);

        if (maxBooksToReturn.HasValue)
        {
            return bookLoansQuery.Take(maxBooksToReturn.Value).ToArray();
        }

        return bookLoansQuery.ToArray();
    }
}
