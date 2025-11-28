using Dapper;
using DataStorage.Entities;
using DataStorage.Exceptions;
using DataStorage.Converters;

namespace DataStorage.Repositories;


public class BookRepository(IDbConnectionFactory connectionFactory) : BaseRepository(connectionFactory), IBookRepository
{

    public async Task<IEnumerable<BusinessModels.Book>> GetAllBooks()
    {
        const string sql = @"
            SELECT
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.Id
            ORDER BY b.Id";

        using var connection = _connectionFactory.CreateConnection();
        var bookDictionary = new Dictionary<int, (Entities.Book book, Entities.Author? author)>();

        await connection.QueryAsync<Entities.Book, Entities.Author, (Entities.Book, Entities.Author?)>(
            sql,
            (book, author) =>
            {
                bookDictionary[book.Id] = (book, author);
                return (book, author);
            },
            splitOn: "Id");

        return bookDictionary.Values.Select(tuple => BookConverter.ToModel(tuple.book, tuple.author));
    }

    public async Task<BusinessModels.Book> GetBookById(int bookId)
    {
        const string sql = @"
            SELECT
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.Id
            WHERE b.Id = @BookId";

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
            splitOn: "Id");

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
            OUTPUT INSERTED.Id
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
            WHERE Id = @Id";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, entity);

            if (rowsAffected == 0)
                throw new BookIdMissingException($"Book with id {book.Id} not found", book.Id);
        }
        catch (BookIdMissingException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new BookIdMissingException($"Book with id {book.Id} not found", book.Id, e);
        }

        return await GetBookById(book.Id);
    }


    public async Task<bool> DeleteBook(int bookId)
    {
        const string sql = @"
            DELETE FROM Book
            WHERE Id = @BookId";

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
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname
            FROM Book b
            LEFT JOIN Author a ON b.AuthorId = a.Id
            WHERE b.Title LIKE '%' + @TitlePattern + '%'
            ORDER BY b.Title";

        using var connection = _connectionFactory.CreateConnection();
        var bookDictionary = new Dictionary<int, (Entities.Book book, Entities.Author? author)>();

        await connection.QueryAsync<Entities.Book, Entities.Author, (Entities.Book, Entities.Author?)>(
            sql,
            (book, author) =>
            {
                bookDictionary[book.Id] = (book, author);
                return (book, author);
            },
            new { TitlePattern = titlePattern },
            splitOn: "Id");

        return bookDictionary.Values.Select(tuple => BookConverter.ToModel(tuple.book, tuple.author));
    }
}
