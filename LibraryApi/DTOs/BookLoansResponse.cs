namespace LibraryApi.DTOs;

/// <summary>
/// Response DTO for book loan statistics
/// </summary>
public class BookLoansResponse
{
    /// <summary>
    /// The book that was borrowed
    /// </summary>
    public required BookDto Book { get; set; }

    /// <summary>
    /// Number of times the book was borrowed
    /// </summary>
    public int LoanCount { get; set; }
}
