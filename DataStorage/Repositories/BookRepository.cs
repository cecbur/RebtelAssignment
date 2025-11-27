using Dapper;
using DataStorage.Entities;

namespace DataStorage.Repositories;

/// <summary>
/// Implementation of IBookRepository using Dapper for database operations
/// Follows Repository pattern and Single Responsibility Principle
/// Uses explicit SQL queries for full control and performance
/// </summary>
public class BookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Gets all books from the database using explicit SQL
    /// </summary>
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        const string sql = @"
            SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
            FROM Book
            ORDER BY BookId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql);
    }

    /// <summary>
    /// Gets a book by its ID using parameterized SQL query
    /// </summary>
    public async Task<Book?> GetBookByIdAsync(int bookId)
    {
        const string sql = @"
            SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
            FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Book>(sql, new { BookId = bookId });
    }

    /// <summary>
    /// Gets all book IDs from the database
    /// </summary>
    public async Task<IEnumerable<int>> GetAllBookIdsAsync()
    {
        const string sql = @"
            SELECT BookId
            FROM Book
            ORDER BY BookId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<int>(sql);
    }

    /// <summary>
    /// Adds a new book to the database using explicit INSERT statement
    /// Returns the book with the generated ID
    /// </summary>
    public async Task<Book> AddBookAsync(Book book)
    {
        if (book == null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        const string sql = @"
            INSERT INTO Book (Title, Author, ISBN, PublicationYear, IsAvailable)
            VALUES (@Title, @Author, @ISBN, @PublicationYear, @IsAvailable);

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        var newBookId = await connection.ExecuteScalarAsync<int>(sql, book);
        book.BookId = newBookId;
        return book;
    }

    /// <summary>
    /// Updates an existing book using explicit UPDATE statement
    /// </summary>
    public async Task<bool> UpdateBookAsync(Book book)
    {
        if (book == null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        const string sql = @"
            UPDATE Book
            SET Title = @Title,
                Author = @Author,
                ISBN = @ISBN,
                PublicationYear = @PublicationYear,
                IsAvailable = @IsAvailable
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, book);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Deletes a book by its ID using explicit DELETE statement
    /// </summary>
    public async Task<bool> DeleteBookAsync(int bookId)
    {
        const string sql = @"
            DELETE FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { BookId = bookId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Searches books by title pattern using SQL LIKE query
    /// </summary>
    public async Task<IEnumerable<Book>> SearchBooksByTitleAsync(string titlePattern)
    {
        if (string.IsNullOrWhiteSpace(titlePattern))
        {
            return await GetAllBooksAsync();
        }

        const string sql = @"
            SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
            FROM Book
            WHERE Title LIKE '%' + @TitlePattern + '%'
            ORDER BY Title";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql, new { TitlePattern = titlePattern });
    }
}
