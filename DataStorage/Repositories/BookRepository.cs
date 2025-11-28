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
            SELECT
                b.BookId, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.AuthorId, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.AuthorId
            ORDER BY b.BookId";

        using var connection = _connectionFactory.CreateConnection();
        var bookDictionary = new Dictionary<int, (Entities.Book book, Entities.Author? author)>();

        await connection.QueryAsync<Entities.Book, Entities.Author, (Entities.Book, Entities.Author?)>(
            sql,
            (book, author) =>
            {
                bookDictionary[book.BookId] = (book, author);
                return (book, author);
            },
            splitOn: "AuthorId");

        return bookDictionary.Values.Select(tuple => BookConverter.ToModel(tuple.book, tuple.author));
    }

    public async Task<BusinessModels.Book> GetBookById(int bookId)
    {
        const string sql = @"
            SELECT
                b.BookId, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.AuthorId, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.AuthorId
            WHERE b.BookId = @BookId";

        using var connection = _connectionFactory.CreateConnection();

        Entities.Book? bookEntity = null;
        Entities.Author? authorEntity = null;

        await connection.QueryAsync<Entities.Book, Entities.Author, int>(
            sql,
            (book, author) =>
            {
                bookEntity = book;
                authorEntity = author;
                return 0;
            },
            new { BookId = bookId },
            splitOn: "AuthorId");

        if (bookEntity == null)
            throw new BookIdMissingException($"Book with id {bookId} not found", bookId);

        return BookConverter.ToModel(bookEntity, authorEntity);
    }

    public async Task<BusinessModels.Book> AddBook(BusinessModels.Book book)
    {
        if (book == null)
            throw new ArgumentNullException(nameof(book));

        var entity = BookConverter.ToEntity(book);

        const string sql = @"
            INSERT INTO Book (Title, AuthorId, ISBN, PublicationYear, NumberOfPages, IsAvailable)
            OUTPUT INSERTED.BookId
            VALUES (@Title, @AuthorId, @ISBN, @PublicationYear, @NumberOfPages, @IsAvailableForLoan);";

        using var connection = _connectionFactory.CreateConnection();
        var newBookId = await connection.QuerySingleAsync<int>(sql, entity);

        return await GetBookById(newBookId);
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
            WHERE BookId = @BookId";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, entity);

            if (rowsAffected == 0)
                throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId);
        }
        catch (BookIdMissingException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new BookIdMissingException($"Book with id {book.BookId} not found", book.BookId, e);
        }

        return await GetBookById(book.BookId);
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
            SELECT
                b.BookId, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.AuthorId, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.AuthorId
            WHERE b.Title LIKE '%' + @TitlePattern + '%'
            ORDER BY b.Title";

        using var connection = _connectionFactory.CreateConnection();
        var bookDictionary = new Dictionary<int, (Entities.Book book, Entities.Author? author)>();

        await connection.QueryAsync<Entities.Book, Entities.Author, (Entities.Book, Entities.Author?)>(
            sql,
            (book, author) =>
            {
                bookDictionary[book.BookId] = (book, author);
                return (book, author);
            },
            new { TitlePattern = titlePattern },
            splitOn: "AuthorId");

        return bookDictionary.Values.Select(tuple => BookConverter.ToModel(tuple.book, tuple.author));
    }
}
