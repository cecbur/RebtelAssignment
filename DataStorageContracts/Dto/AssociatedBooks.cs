using BusinessModels;

namespace DataStorageContracts.Dto;

public class AssociatedBooks
{
    public Book Book { get; set; }

    public BookCount[] Associated { get; set; }
    
    public class BookCount
    {
        public Book Book { get; set; }
        public int Count { get; set; }
    }
}
