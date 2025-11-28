using Dapper;
using DataStorage.Repositories;
using DataStorageContracts;
using DataStorageContracts.Dto;

namespace DataStorage.RepositoriesMultipleTables;


public class BorrowingPatternRepository(IDbConnectionFactory connectionFactory, IBookRepository bookRepository) : BaseRepository(connectionFactory), IBorrowingPatternRepository
{
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
    /// <summary>
    /// Gets other books that were borrowed by patrons who borrowed this book
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns>Books and frequencies</returns>
    public async Task<AssociatedBooks> GetOtherBooksBorrowed(int bookId)
    {
        // Query to get associated book IDs with their borrow counts
        const string associatedSql = @"
            SELECT
                otherBook.Id as BookId,
                COUNT(otherBook.Id) as BorrowCount
            FROM Loan l
            INNER JOIN Patron p ON l.PatronId = p.Id
            INNER JOIN Loan otherLoan ON otherLoan.PatronId = p.Id
            INNER JOIN Book otherBook ON otherLoan.BookId = otherBook.Id
            WHERE l.BookId = @BookId
                AND otherBook.Id <> @BookId
            GROUP BY otherBook.Id
            ORDER BY BorrowCount DESC";

        using var connection = _connectionFactory.CreateConnection();

        // Get associated book IDs with counts
        var associatedBookData = await connection.QueryAsync<(int BookId, int BorrowCount)>(
            associatedSql,
            new { BookId = bookId });

        var associatedList = associatedBookData.ToList();

        // Get all associated books from BookRepository
        var associatedBookIds = associatedList.Select(x => x.BookId).ToList();
        var books = await _bookRepository.GetBooksByIds(associatedBookIds.Concat([bookId]));
        var bookArray = books.ToArray();
        var mainBook = bookArray.First(b => b.Id == bookId); 
        
        var bookDictionary = bookArray.Except([mainBook]).ToDictionary(b => b.Id);

        // Build the result
        var associatedBooksList = new List<AssociatedBooks.BookCount>();
        foreach (var (bookIdValue, count) in associatedList)
        {
            if (bookDictionary.TryGetValue(bookIdValue, out var book))
            {
                associatedBooksList.Add(new AssociatedBooks.BookCount
                {
                    Book = book,
                    Count = count
                });
            }
        }

        return new AssociatedBooks
            {
                Book = mainBook,
                Associated = associatedBooksList.ToArray()
            };
    }
}
