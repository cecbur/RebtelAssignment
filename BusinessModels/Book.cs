namespace BusinessModels;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int? AuthorId { get; set; }

    public string? Isbn { get; set; }

    public int? PublicationYear { get; set; }

    public int? NumberOfPages { get; set; }

    public bool IsAvailableForLoan { get; set; } = true;
}
