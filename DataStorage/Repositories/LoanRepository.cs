using Dapper;
using DataStorage.Entities;
using DataStorage.Converters;
using DataStorageContracts;

namespace DataStorage.Repositories;

public class LoanRepository(IDbConnectionFactory connectionFactory) : BaseRepository(connectionFactory), ILoanRepository
{

    public async Task<IEnumerable<BusinessModels.Loan>> GetAllLoans()
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            ORDER BY l.LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var loanDictionary = new Dictionary<int, (Entities.Loan loan, Entities.Book? book, Entities.Author? author, Entities.Patron? patron)>();

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanDictionary[loan.Id] = (loan, book, author, patron);
                return 0;
            },
            splitOn: "Id,Id,Id");

        return loanDictionary.Values.Select(tuple => LoanConverter.ToModel(tuple.loan, tuple.book, tuple.author, tuple.patron));
    }

    public async Task<BusinessModels.Loan> GetLoanById(int loanId)
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            WHERE l.Id = @LoanId";

        using var connection = _connectionFactory.CreateConnection();

        Entities.Loan? loanEntity = null;
        Entities.Book? bookEntity = null;
        Entities.Author? authorEntity = null;
        Entities.Patron? patronEntity = null;

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanEntity = loan;
                bookEntity = book;
                authorEntity = author;
                patronEntity = patron;
                return 0;
            },
            new { LoanId = loanId },
            splitOn: "Id,Id,Id");

        if (loanEntity == null)
            throw new InvalidOperationException($"Loan with id {loanId} not found");

        return LoanConverter.ToModel(loanEntity, bookEntity, authorEntity, patronEntity);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByPatronId(int patronId)
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            WHERE l.PatronId = @PatronId
            ORDER BY l.LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var loanDictionary = new Dictionary<int, (Entities.Loan loan, Entities.Book? book, Entities.Author? author, Entities.Patron? patron)>();

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanDictionary[loan.Id] = (loan, book, author, patron);
                return 0;
            },
            new { PatronId = patronId },
            splitOn: "Id,Id,Id");

        return loanDictionary.Values.Select(tuple => LoanConverter.ToModel(tuple.loan, tuple.book, tuple.author, tuple.patron));
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByBookId(int bookId)
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            WHERE l.BookId = @BookId
            ORDER BY l.LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var loanDictionary = new Dictionary<int, (Entities.Loan loan, Entities.Book? book, Entities.Author? author, Entities.Patron? patron)>();

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanDictionary[loan.Id] = (loan, book, author, patron);
                return 0;
            },
            new { BookId = bookId },
            splitOn: "Id,Id,Id");

        return loanDictionary.Values.Select(tuple => LoanConverter.ToModel(tuple.loan, tuple.book, tuple.author, tuple.patron));
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetActiveLoans()
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            WHERE l.IsReturned = 0
            ORDER BY l.DueDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        var loanDictionary = new Dictionary<int, (Entities.Loan loan, Entities.Book? book, Entities.Author? author, Entities.Patron? patron)>();

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanDictionary[loan.Id] = (loan, book, author, patron);
                return 0;
            },
            splitOn: "Id,Id,Id");

        return loanDictionary.Values.Select(tuple => LoanConverter.ToModel(tuple.loan, tuple.book, tuple.author, tuple.patron));
    }

    public async Task<BusinessModels.Loan> AddLoan(BusinessModels.Loan loan)
    {
        if (loan == null)
            throw new ArgumentNullException(nameof(loan));

        var entity = LoanConverter.ToEntity(loan);

        const string sql = @"
            INSERT INTO Loan (BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned)
            OUTPUT INSERTED.Id, INSERTED.BookId, INSERTED.PatronId, INSERTED.LoanDate, INSERTED.DueDate, INSERTED.ReturnDate, INSERTED.IsReturned
            VALUES (@BookId, @PatronId, @LoanDate, @DueDate, @ReturnDate, @IsReturned);";

        using var connection = _connectionFactory.CreateConnection();
        var newEntity = await connection.QuerySingleAsync<Entities.Loan>(sql, entity);

        // Fetch the complete loan with all joined objects
        return await GetLoanById(newEntity.Id);
    }

    public async Task<BusinessModels.Loan> UpdateLoan(BusinessModels.Loan loan)
    {
        var entity = LoanConverter.ToEntity(loan);

        const string sql = @"
            UPDATE Loan
            SET BookId = @BookId,
                PatronId = @PatronId,
                LoanDate = @LoanDate,
                DueDate = @DueDate,
                ReturnDate = @ReturnDate,
                IsReturned = @IsReturned
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, entity);

        if (rowsAffected == 0)
            throw new InvalidOperationException($"Loan with id {loan.Id} not found");

        // Fetch the complete loan with all joined objects
        return await GetLoanById(loan.Id);
    }

    public async Task<bool> DeleteLoan(int loanId)
    {
        const string sql = @"
            DELETE FROM Loan
            WHERE Id = @LoanId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { LoanId = loanId });
        return rowsAffected > 0;
    }
}
