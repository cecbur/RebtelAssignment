using DataStorageContracts.Dto;

namespace DataStorageContracts;

public interface IBorrowingPatternRepository
{
    Task<AssociatedBooks> GetOtherBooksBorrowed(int bookId);
}
