namespace LibraryApi.DTOs;

/// <summary>
/// Data Transfer Object for Author entity
/// Separates API model from database model (DTO pattern)
/// </summary>
public class AuthorDto
{
    public int Id { get; set; }
    public string? GivenName { get; set; }
    public string Surname { get; set; } = string.Empty;
}
