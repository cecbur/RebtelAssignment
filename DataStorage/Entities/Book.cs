namespace DataStorage.Entities;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public string? Isbn { get; set; }

    public int? PublicationYear { get; set; }

    public bool IsAvailableForLoan { get; set; } = true;
}
