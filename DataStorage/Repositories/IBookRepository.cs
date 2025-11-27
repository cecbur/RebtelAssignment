using DataStorage.Entities;

namespace DataStorage.Repositories;


public interface IBookRepository
{
    /// Gets all books from the database
    Task<IEnumerable<Book>> GetAllBooks();

    /// <summary>
    /// Gets a book by its ID
    /// </summary>
    /// <param name="bookId">The ID of the book to retrieve</param>
    /// <returns>The book if found, null otherwise</returns>
    Task<Book> GetBookById(int bookId);

    /// Adds a new book to the database
    /// <returns>The added book with generated ID</returns>
    Task<Book> AddBookAsync(Book book);

    /// <summary>
    /// Updates an existing book
    /// </summary>
    /// <param name="book">The book to update</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<Book> UpdateBookAsync(Book book);

    /// <summary>
    /// Deletes a book by its ID
    /// </summary>
    /// <param name="bookId">The ID of the book to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteBookAsync(int bookId);

    /// <summary>
    /// Gets books by title pattern
    /// </summary>
    /// <param name="titlePattern">The title pattern to use with SQL LIKE</param>
    /// <returns>Collection of matching books</returns>
    Task<IEnumerable<Book>> SearchBooksByTitleLikeQuery(string titlePattern);
}
