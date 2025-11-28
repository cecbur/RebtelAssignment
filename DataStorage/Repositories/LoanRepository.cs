using Dapper;
using DataStorage.Entities;

namespace DataStorage.Repositories;

public class LoanRepository(IDbConnectionFactory connectionFactory) : ILoanRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<Loan>> GetAllLoans()
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Loan>(sql);
    }

    public async Task<Loan> GetLoanById(int loanId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE LoanId = @LoanId";

        using var connection = _connectionFactory.CreateConnection();
        var loan = await connection.QuerySingleOrDefaultAsync<Loan>(sql, new { LoanId = loanId });

        if (loan == null)
            throw new InvalidOperationException($"Loan with id {loanId} not found");

        return loan;
    }

    public async Task<IEnumerable<Loan>> GetLoansByPatronId(int patronId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE PatronId = @PatronId
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Loan>(sql, new { PatronId = patronId });
    }

    public async Task<IEnumerable<Loan>> GetLoansByBookId(int bookId)
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE BookId = @BookId
            ORDER BY LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Loan>(sql, new { BookId = bookId });
    }

    public async Task<IEnumerable<Loan>> GetActiveLoans()
    {
        const string sql = @"
            SELECT LoanId, BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned
            FROM Loan
            WHERE IsReturned = 0
            ORDER BY DueDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Loan>(sql);
    }

    public async Task<Loan> AddLoan(Loan loan)
    {
        if (loan == null)
            throw new ArgumentNullException(nameof(loan));

        const string sql = @"
            INSERT INTO Loan (BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned)
            OUTPUT INSERTED.LoanId, INSERTED.BookId, INSERTED.PatronId, INSERTED.LoanDate, INSERTED.DueDate, INSERTED.ReturnDate, INSERTED.IsReturned
            VALUES (@BookId, @PatronId, @LoanDate, @DueDate, @ReturnDate, @IsReturned);";

        using var connection = _connectionFactory.CreateConnection();
        var newLoan = await connection.QuerySingleAsync<Loan>(sql, loan);
        return newLoan;
    }

    public async Task<Loan> UpdateLoan(Loan loan)
    {
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
        var updatedLoan = await connection.QuerySingleOrDefaultAsync<Loan>(sql, loan);

        if (updatedLoan == null)
            throw new InvalidOperationException($"Loan with id {loan.LoanId} not found");

        return updatedLoan;
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
