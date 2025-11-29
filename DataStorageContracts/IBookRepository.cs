using BusinessModels;

namespace DataStorageContracts;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooks();

    Task<Book> GetBookById(int bookId);
    
    Task<IEnumerable<Book>> GetBooksByIds(IEnumerable<int> bookIds);

    /// Adds a new book to the database
    /// <returns>The book from the database</returns>
    Task<Book> AddBook(Book book);

    /// Updates an existing book
    /// <returns>The updated book from the database</returns>
    Task<Book> UpdateBook(Book book);

    Task<bool> DeleteBook(int bookId);

    /// <summary>
    /// Gets books by title pattern
    /// </summary>
    /// <param name="titlePattern">The title pattern to use with SQL LIKE</param>
    /// <returns>Collection of matching books</returns>
    Task<IEnumerable<Book>> SearchBooksByTitleLikeQuery(string titlePattern);

}
