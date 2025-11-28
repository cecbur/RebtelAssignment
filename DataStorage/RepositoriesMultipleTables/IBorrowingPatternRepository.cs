using DataStorageContracts.Dto;

namespace DataStorage.RepositoriesMultipleTables;


public interface IBorrowingPatternRepository
{
    Task<IEnumerable<AssociatedBooks>> GetOtherBooksBorrowed(int bookId);
}
