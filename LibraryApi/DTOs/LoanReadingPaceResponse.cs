namespace LibraryApi.DTOs;

public class LoanReadingPaceResponse
{
    public int LoanId { get; set; }
    public double? PagesPerDay { get; set; }
    public string? Message { get; set; }
}
