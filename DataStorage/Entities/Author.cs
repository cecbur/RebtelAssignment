namespace DataStorage.Entities;

internal class Author
{
    public int AuthorId { get; set; }

    public string? GivenName { get; set; }

    public string Surname { get; set; } = string.Empty;
}
