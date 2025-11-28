using DataStorage.Entities;

namespace DataStorage.Repositories;

public interface IPatronRepository
{
    Task<IEnumerable<Patron>> GetAllPatrons();
    Task<Patron> GetPatronById(int patronId);
    Task<Patron> AddPatron(Patron patron);
    Task<Patron> UpdatePatron(Patron patron);
    Task<bool> DeletePatron(int patronId);
    Task<Patron?> GetPatronByEmail(string email);
}
