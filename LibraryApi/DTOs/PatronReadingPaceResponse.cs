namespace LibraryApi.DTOs;

public class PatronReadingPaceResponse
{
    public int PatronId { get; set; }
    public double? PagesPerDay { get; set; }
    public string? Message { get; set; }
}
