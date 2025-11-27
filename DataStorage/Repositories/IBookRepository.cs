using DataStorage.Entities;

namespace DataStorage.Repositories;

/// <summary>
/// Repository interface for Book entity operations
/// Follows Repository pattern for data access abstraction
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Gets all books from the database
    /// </summary>
    /// <returns>Collection of all books</returns>
    Task<IEnumerable<Book>> GetAllBooksAsync();

    /// <summary>
    /// Gets a book by its ID
    /// </summary>
    /// <param name="bookId">The ID of the book to retrieve</param>
    /// <returns>The book if found, null otherwise</returns>
    Task<Book?> GetBookByIdAsync(int bookId);

    /// <summary>
    /// Gets all book IDs from the database
    /// </summary>
    /// <returns>Collection of book IDs</returns>
    Task<IEnumerable<int>> GetAllBookIdsAsync();

    /// <summary>
    /// Adds a new book to the database
    /// </summary>
    /// <param name="book">The book to add</param>
    /// <returns>The added book with generated ID</returns>
    Task<Book> AddBookAsync(Book book);

    /// <summary>
    /// Updates an existing book
    /// </summary>
    /// <param name="book">The book to update</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateBookAsync(Book book);

    /// <summary>
    /// Deletes a book by its ID
    /// </summary>
    /// <param name="bookId">The ID of the book to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteBookAsync(int bookId);

    /// <summary>
    /// Gets books by title pattern
    /// </summary>
    /// <param name="titlePattern">The title pattern to search for</param>
    /// <returns>Collection of matching books</returns>
    Task<IEnumerable<Book>> SearchBooksByTitleAsync(string titlePattern);
}
