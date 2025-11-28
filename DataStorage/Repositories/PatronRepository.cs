using Dapper;
using DataStorage.Entities;

namespace DataStorage.Repositories;

public class PatronRepository(IDbConnectionFactory connectionFactory) : IPatronRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<Patron>> GetAllPatrons()
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            ORDER BY LastName, FirstName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Patron>(sql);
    }

    public async Task<Patron> GetPatronById(int patronId)
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            WHERE PatronId = @PatronId";

        using var connection = _connectionFactory.CreateConnection();
        var patron = await connection.QuerySingleOrDefaultAsync<Patron>(sql, new { PatronId = patronId });

        if (patron == null)
            throw new InvalidOperationException($"Patron with id {patronId} not found");

        return patron;
    }

    public async Task<Patron?> GetPatronByEmail(string email)
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            WHERE Email = @Email";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Patron>(sql, new { Email = email });
    }

    public async Task<Patron> AddPatron(Patron patron)
    {
        if (patron == null)
            throw new ArgumentNullException(nameof(patron));

        const string sql = @"
            INSERT INTO Patron (FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive)
            OUTPUT INSERTED.PatronId, INSERTED.FirstName, INSERTED.LastName, INSERTED.Email, INSERTED.PhoneNumber, INSERTED.MembershipDate, INSERTED.IsActive
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @MembershipDate, @IsActive);";

        using var connection = _connectionFactory.CreateConnection();
        var newPatron = await connection.QuerySingleAsync<Patron>(sql, patron);
        return newPatron;
    }

    public async Task<Patron> UpdatePatron(Patron patron)
    {
        const string sql = @"
            UPDATE Patron
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                MembershipDate = @MembershipDate,
                IsActive = @IsActive
            OUTPUT INSERTED.PatronId, INSERTED.FirstName, INSERTED.LastName, INSERTED.Email, INSERTED.PhoneNumber, INSERTED.MembershipDate, INSERTED.IsActive
            WHERE PatronId = @PatronId";

        using var connection = _connectionFactory.CreateConnection();
        var updatedPatron = await connection.QuerySingleOrDefaultAsync<Patron>(sql, patron);

        if (updatedPatron == null)
            throw new InvalidOperationException($"Patron with id {patron.PatronId} not found");

        return updatedPatron;
    }

    public async Task<bool> DeletePatron(int patronId)
    {
        const string sql = @"
            DELETE FROM Patron
            WHERE PatronId = @PatronId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { PatronId = patronId });
        return rowsAffected > 0;
    }
}
