namespace LibraryApi.DTOs;

/// <summary>
/// Response DTO for book borrowing frequency analysis
/// </summary>
public class BookFrequencyResponse
{
    /// <summary>
    /// The associated book that was borrowed by patrons who borrowed the main book
    /// </summary>
    public required BookDto AssociatedBook { get; set; }

    /// <summary>
    /// Ratio of loans for this book compared to the main book
    /// (Number of times this book was borrowed / Total loans of main book)
    /// </summary>
    public double LoansOfThisBookPerLoansOfMainBook { get; set; }
}
