using Dapper;
using DataStorage.Entities;
using DataStorage.Exceptions;
using DataStorage.Converters;

namespace DataStorage.Repositories;


public class BookRepository(IDbConnectionFactory connectionFactory) : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<BusinessModels.Book>> GetAllBooks()
    {
        const string sql = @"
            SELECT BookId, Title, AuthorId, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            ORDER BY BookId";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Book>(sql);
        return entities.Select(BookConverter.ToModel);
    }

    public async Task<BusinessModels.Book> GetBookById(int bookId)
    {
        const string sql = @"
            SELECT BookId, Title, AuthorId, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            WHERE BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Book>(sql, new { BookId = bookId });

        if (entity == null)
            throw new BookIdMissingException($"Book with id {bookId} not found", bookId);

        return BookConverter.ToModel(entity);
    }

    public async Task<BusinessModels.Book> AddBook(BusinessModels.Book book)
    {
        if (book == null)
            throw new ArgumentNullException(nameof(book));

        var entity = BookConverter.ToEntity(book);

        const string sql = @"
            INSERT INTO Book (Title, AuthorId, ISBN, PublicationYear, NumberOfPages, IsAvailable)
            OUTPUT INSERTED.BookId, INSERTED.Title, INSERTED.AuthorId, INSERTED.ISBN, INSERTED.PublicationYear, INSERTED.NumberOfPages, INSERTED.IsAvailable
            VALUES (@Title, @AuthorId, @ISBN, @PublicationYear, @NumberOfPages, @IsAvailableForLoan);";

        using var connection = _connectionFactory.CreateConnection();
        var newEntity = await connection.QuerySingleAsync<Entities.Book>(sql, entity);
        return BookConverter.ToModel(newEntity);
    }


    public async Task<BusinessModels.Book> UpdateBook(BusinessModels.Book book)
    {
        var entity = BookConverter.ToEntity(book);

        const string sql = @"
            UPDATE Book
            SET Title = @Title,
                AuthorId = @AuthorId,
                ISBN = @ISBN,
                PublicationYear = @PublicationYear,
                NumberOfPages = @NumberOfPages,
                IsAvailable = @IsAvailableForLoan
            OUTPUT INSERTED.BookId, INSERTED.Title, INSERTED.AuthorId, INSERTED.ISBN, INSERTED.PublicationYear, INSERTED.NumberOfPages, INSERTED.IsAvailable
            WHERE BookId = @BookId";

        Entities.Book? updatedEntity;
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            updatedEntity = await connection.QuerySingleOrDefaultAsync<Entities.Book>(sql, entity);
        }
        catch (Exception e)
        {
            throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId, e);
        }

        if (updatedEntity == null)
            throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId);

        return BookConverter.ToModel(updatedEntity);
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

    public async Task<IEnumerable<BusinessModels.Book>> SearchBooksByTitleLikeQuery(string titlePattern)
    {
        if (string.IsNullOrWhiteSpace(titlePattern))
        {
            return await GetAllBooks();
        }

        const string sql = @"
            SELECT BookId, Title, AuthorId, ISBN, PublicationYear, NumberOfPages, IsAvailable
            FROM Book
            WHERE Title LIKE '%' + @TitlePattern + '%'
            ORDER BY Title";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Book>(sql, new { TitlePattern = titlePattern });
        return entities.Select(BookConverter.ToModel);
    }
}
