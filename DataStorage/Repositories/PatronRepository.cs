using Dapper;
using DataStorage.Entities;
using DataStorage.Converters;

namespace DataStorage.Repositories;

public class PatronRepository(IDbConnectionFactory connectionFactory) : IPatronRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<BusinessModels.Patron>> GetAllPatrons()
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            ORDER BY LastName, FirstName";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Patron>(sql);
        return entities.Select(PatronConverter.ToModel);
    }

    public async Task<BusinessModels.Patron> GetPatronById(int patronId)
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            WHERE PatronId = @PatronId";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Patron>(sql, new { PatronId = patronId });

        if (entity == null)
            throw new InvalidOperationException($"Patron with id {patronId} not found");

        return PatronConverter.ToModel(entity);
    }

    public async Task<BusinessModels.Patron?> GetPatronByEmail(string email)
    {
        const string sql = @"
            SELECT PatronId, FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive
            FROM Patron
            WHERE Email = @Email";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Patron>(sql, new { Email = email });
        return entity != null ? PatronConverter.ToModel(entity) : null;
    }

    public async Task<BusinessModels.Patron> AddPatron(BusinessModels.Patron patron)
    {
        if (patron == null)
            throw new ArgumentNullException(nameof(patron));

        var entity = PatronConverter.ToEntity(patron);

        const string sql = @"
            INSERT INTO Patron (FirstName, LastName, Email, PhoneNumber, MembershipDate, IsActive)
            OUTPUT INSERTED.PatronId, INSERTED.FirstName, INSERTED.LastName, INSERTED.Email, INSERTED.PhoneNumber, INSERTED.MembershipDate, INSERTED.IsActive
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @MembershipDate, @IsActive);";

        using var connection = _connectionFactory.CreateConnection();
        var newEntity = await connection.QuerySingleAsync<Entities.Patron>(sql, entity);
        return PatronConverter.ToModel(newEntity);
    }

    public async Task<BusinessModels.Patron> UpdatePatron(BusinessModels.Patron patron)
    {
        var entity = PatronConverter.ToEntity(patron);

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
        var updatedEntity = await connection.QuerySingleOrDefaultAsync<Entities.Patron>(sql, entity);

        if (updatedEntity == null)
            throw new InvalidOperationException($"Patron with id {patron.Id} not found");

        return PatronConverter.ToModel(updatedEntity);
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
