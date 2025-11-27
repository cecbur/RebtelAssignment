namespace LibraryApi.DTOs;

/// <summary>
/// Response DTO for borrowing patterns analysis
/// </summary>
public class BorrowingPatternsResponse
{
    /// <summary>
    /// Book IDs that are powers of two
    /// </summary>
    public List<int> PowerOfTwoBookIds { get; set; } = new();

    /// <summary>
    /// Book IDs that are odd numbers
    /// </summary>
    public List<int> OddNumberedBookIds { get; set; } = new();

    /// <summary>
    /// Total number of books analyzed
    /// </summary>
    public int TotalBooksAnalyzed { get; set; }
}
