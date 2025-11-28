namespace LibraryApi.DTOs;

public class LoneReadingPaceResponse
{
    public int LoneId { get; set; }
    public double? PagesPerDay { get; set; }
    public string? Message { get; set; }
}
