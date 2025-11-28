using Dapper;
using DataStorage.Entities;
using DataStorage.Exceptions;

namespace DataStorage.Repositories;


public class BookRepository(IDbConnectionFactory connectionFactory) : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    
    public async Task<IEnumerable<Book>> GetAllBooks()
    {
        const string sql = @"
            SELECT BookId, Title, AuthorGivenName, AuthorSurname, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            ORDER BY BookId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<Book> GetBookById(int bookId)
    {
        const string sql = @"
            SELECT BookId, Title, AuthorGivenName, AuthorSurname, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var book = await connection.QuerySingleOrDefaultAsync<Book>(sql, new { BookId = bookId });

        if (book == null)
            throw new BookIdMissingException($"Book with id {bookId} not found", bookId);

        return book;
    }

    public async Task<Book> AddBook(Book book)
    {
        if (book == null)
            throw new ArgumentNullException(nameof(book));

        const string sql = @"
            INSERT INTO Book (Title, AuthorGivenName, AuthorSurname, ISBN, PublicationYear, NumberOfPages, IsAvailable)
            OUTPUT INSERTED.BookId, INSERTED.Title, INSERTED.AuthorGivenName, INSERTED.AuthorSurname, INSERTED.ISBN, INSERTED.PublicationYear, INSERTED.NumberOfPages, INSERTED.IsAvailable
            VALUES (@Title, @AuthorGivenName, @AuthorSurname, @ISBN, @PublicationYear, @NumberOfPages, @IsAvailable);";

        using var connection = _connectionFactory.CreateConnection();
        var newBook = await connection.QuerySingleAsync<Book>(sql, book);
        return newBook;
    }


    public async Task<Book> UpdateBook(Book book)
    {
        const string sql = @"
            UPDATE Book
            SET Title = @Title,
                AuthorGivenName = @AuthorGivenName,
                AuthorSurname = @AuthorSurname,
                ISBN = @ISBN,
                PublicationYear = @PublicationYear,
                NumberOfPages = @NumberOfPages,
                IsAvailable = @IsAvailable
            OUTPUT INSERTED.BookId, INSERTED.Title, INSERTED.AuthorGivenName, INSERTED.AuthorSurname, INSERTED.ISBN, INSERTED.PublicationYear, INSERTED.NumberOfPages, INSERTED.IsAvailable
            WHERE BookId = @BookId";

        Book? updatedBook;
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            updatedBook = await connection.QuerySingleOrDefaultAsync<Book>(sql, book);
        }
        catch (Exception e)
        {
            throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId, e);
        }

        if (updatedBook == null)
            throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId);

        return updatedBook;
    }


    public async Task<bool> DeleteBook(int bookId)
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
            SELECT BookId, Title, AuthorGivenName, AuthorSurname, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            WHERE Title LIKE '%' + @TitlePattern + '%'
            ORDER BY Title";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql, new { TitlePattern = titlePattern });
    }
}
