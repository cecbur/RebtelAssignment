namespace LibraryApi.DTOs;

/// <summary>
/// Data Transfer Object for Book entity
/// Separates API model from database model (DTO pattern)
/// </summary>
public class BookDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public int? PublicationYear { get; set; }
    public bool IsAvailable { get; set; }
}
