namespace LibraryApi.DTOs;

public class PatronLoanFrequencyResponse
{
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}
