using Dapper;
using DataStorage.Entities;
using DataStorage.Converters;

namespace DataStorage.Repositories;

public class AuthorRepository(IDbConnectionFactory connectionFactory) : IAuthorRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<BusinessModels.Author>> GetAllAuthors()
    {
        const string sql = @"
            SELECT AuthorId, GivenName, Surname
            FROM Author
            ORDER BY Surname, GivenName";

        using var connection = _connectionFactory.CreateConnection();
        var entities = await connection.QueryAsync<Entities.Author>(sql);
        return entities.Select(AuthorConverter.ToModel);
    }

    public async Task<BusinessModels.Author> GetAuthorById(int authorId)
    {
        const string sql = @"
            SELECT AuthorId, GivenName, Surname
            FROM Author
            WHERE AuthorId = @AuthorId";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Author>(sql, new { AuthorId = authorId });

        if (entity == null)
            throw new InvalidOperationException($"Author with id {authorId} not found");

        return AuthorConverter.ToModel(entity);
    }

    public async Task<BusinessModels.Author?> GetAuthorBySurname(string surname)
    {
        const string sql = @"
            SELECT AuthorId, GivenName, Surname
            FROM Author
            WHERE Surname = @Surname";

        using var connection = _connectionFactory.CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<Entities.Author>(sql, new { Surname = surname });
        return entity != null ? AuthorConverter.ToModel(entity) : null;
    }

    public async Task<BusinessModels.Author> AddAuthor(BusinessModels.Author author)
    {
        if (author == null)
            throw new ArgumentNullException(nameof(author));

        var entity = AuthorConverter.ToEntity(author);

        const string sql = @"
            INSERT INTO Author (GivenName, Surname)
            OUTPUT INSERTED.AuthorId, INSERTED.GivenName, INSERTED.Surname
            VALUES (@GivenName, @Surname);";

        using var connection = _connectionFactory.CreateConnection();
        var newEntity = await connection.QuerySingleAsync<Entities.Author>(sql, entity);
        return AuthorConverter.ToModel(newEntity);
    }

    public async Task<BusinessModels.Author> UpdateAuthor(BusinessModels.Author author)
    {
        var entity = AuthorConverter.ToEntity(author);

        const string sql = @"
            UPDATE Author
            SET GivenName = @GivenName,
                Surname = @Surname
            OUTPUT INSERTED.AuthorId, INSERTED.GivenName, INSERTED.Surname
            WHERE AuthorId = @AuthorId";

        using var connection = _connectionFactory.CreateConnection();
        var updatedEntity = await connection.QuerySingleOrDefaultAsync<Entities.Author>(sql, entity);

        if (updatedEntity == null)
            throw new InvalidOperationException($"Author with id {author.AuthorId} not found");

        return AuthorConverter.ToModel(updatedEntity);
    }

    public async Task<bool> DeleteAuthor(int authorId)
    {
        const string sql = @"
            DELETE FROM Author
            WHERE AuthorId = @AuthorId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { AuthorId = authorId });
        return rowsAffected > 0;
    }
}
