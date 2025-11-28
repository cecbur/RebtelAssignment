namespace BusinessModels;

public class Author
{
    public int Id { get; set; }

    public string? GivenName { get; set; }

    public string Surname { get; set; } = string.Empty;
    
    public string Name => $"{GivenName} {Surname}";
}
