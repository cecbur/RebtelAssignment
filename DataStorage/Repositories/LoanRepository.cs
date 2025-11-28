using Dapper;
using DataStorage.Entities;
using DataStorage.Converters;

namespace DataStorage.Repositories;

public class LoanRepository(IDbConnectionFactory connectionFactory) : ILoanRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<BusinessModels.Loan>> GetAllLoans()
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Loan>(sql);
        return entities.Select(LoanConverter.ToModel);
    }

    public async Task<BusinessModels.Loan> GetLoanById(int loanId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE LoanId = @LoanId";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Loan>(sql, new { LoanId = loanId });

        if (entity == null)
            throw new InvalidOperationException($"Loan with id {loanId} not found");

        return LoanConverter.ToModel(entity);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByPatronId(int patronId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE PatronId = @PatronId
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Loan>(sql, new { PatronId = patronId });
        return entities.Select(LoanConverter.ToModel);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByBookId(int bookId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE BookId = @BookId
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Loan>(sql, new { BookId = bookId });
        return entities.Select(LoanConverter.ToModel);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetActiveLoans()
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE IsReturned = 0
            ORDER BY DueDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Loan>(sql);
        return entities.Select(LoanConverter.ToModel);
    }

    public async Task<BusinessModels.Loan> AddLoan(BusinessModels.Loan loan)
    {
        if (loan == null)
            throw new ArgumentNullException(nameof(loan));

        var entity = LoanConverter.ToEntity(loan);

        const string sql = @"
            INSERT INTO Loan (BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned)
            OUTPUT INSERTED.LoanId, INSERTED.BookId, INSERTED.PatronId, INSERTED.LoanDate, INSERTED.DueDate, INSERTED.ReturnDate, INSERTED.IsReturned
            VALUES (@BookId, @PatronId, @LoanDate, @DueDate, @ReturnDate, @IsReturned);";

        using var connection = _connectionFactory.CreateConnection();
        var newEntity = await connection.QuerySingleAsync<Entities.Loan>(sql, entity);
        return LoanConverter.ToModel(newEntity);
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
            OUTPUT INSERTED.LoanId, INSERTED.BookId, INSERTED.PatronId, INSERTED.LoanDate, INSERTED.DueDate, INSERTED.ReturnDate, INSERTED.IsReturned
            WHERE LoanId = @LoanId";

        using var connection = _connectionFactory.CreateConnection();
        var updatedEntity = await connection.QuerySingleOrDefaultAsync<Entities.Loan>(sql, entity);

        if (updatedEntity == null)
            throw new InvalidOperationException($"Loan with id {loan.LoanId} not found");

        return LoanConverter.ToModel(updatedEntity);
    }

    public async Task<bool> DeleteLoan(int loanId)
    {
        const string sql = @"
            DELETE FROM Loan
            WHERE LoanId = @LoanId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { LoanId = loanId });
        return rowsAffected > 0;
    }
}
