using Dapper;
using DataStorage.Entities;
using DataStorage.Exceptions;

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
    public async Task<IEnumerable<Book>> GetAllBooks()
    {
        const string sql = @"
            SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
            FROM Book
            ORDER BY BookId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<Book> GetBookById(int bookId)
    {
        const string sql = @"
            SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
            FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var book = await connection.QuerySingleOrDefaultAsync<Book>(sql, new { BookId = bookId });

        if (book == null)
            throw new BookIdMissingException($"Book with id {bookId} not found", bookId);
        
        return book;
    }

    public async Task<Book> AddBookAsync(Book book)
    {
        if (book == null)
            throw new ArgumentNullException(nameof(book));

        const string sql = @"
            INSERT INTO Book (Title, Author, ISBN, PublicationYear, IsAvailable)
            OUTPUT INSERTED
            VALUES (@Title, @Author, @ISBN, @PublicationYear, @IsAvailable);;";

        using var connection = _connectionFactory.CreateConnection();
        var newBook = await connection.ExecuteScalarAsync<Book>(sql, book);
        return newBook!;
    }


    public async Task<Book> UpdateBookAsync(Book book)
    {
        const string sql = @"
            UPDATE Book
            OUTPUT INSERTED
            SET Title = @Title,
                Author = @Author,
                ISBN = @ISBN,
                PublicationYear = @PublicationYear,
                IsAvailable = @IsAvailable
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var updatedBook = await connection.ExecuteScalarAsync<Book>(sql, book);
        return updatedBook!;
    }


    public async Task<bool> DeleteBookAsync(int bookId)
    {
        const string sql = @"
            DELETE FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { BookId = bookId });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Book>> SearchBooksByTitleLikeQuery(string titlePattern)
    {
        if (string.IsNullOrWhiteSpace(titlePattern))
        {
            return await GetAllBooks();
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
