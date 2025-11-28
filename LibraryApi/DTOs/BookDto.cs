namespace LibraryApi.DTOs;

/// <summary>
/// Data Transfer Object for Book entity
/// Separates API model from database model (DTO pattern)
/// </summary>
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public AuthorDto? Author { get; set; }
    public string? ISBN { get; set; }
    public int? PublicationYear { get; set; }
    public int? NumberOfPages { get; set; }
    public bool IsAvailable { get; set; }
}
