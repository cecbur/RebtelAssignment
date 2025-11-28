using BusinessModels;

namespace DataStorage.Repositories;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAuthors();
    Task<Author> GetAuthorById(int authorId);
    Task<Author> AddAuthor(Author author);
    Task<Author> UpdateAuthor(Author author);
    Task<bool> DeleteAuthor(int authorId);
    Task<Author?> GetAuthorBySurname(string surname);
}
