namespace LibraryApi.DTOs;

/// <summary>
/// Data Transfer Object for Loan information
/// </summary>
public class LoanDto
{
    /// <summary>
    /// Unique identifier for the loan
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The book that was loaned
    /// </summary>
    public required BookDto Book { get; set; }

    /// <summary>
    /// The patron who borrowed the book
    /// </summary>
    public required PatronDto Patron { get; set; }

    /// <summary>
    /// Date when the book was loaned
    /// </summary>
    public DateTime LoanDate { get; set; }

    /// <summary>
    /// Date when the book is due to be returned
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date when the book was actually returned (null if not yet returned)
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Indicates if the book has been returned
    /// </summary>
    public bool IsReturned { get; set; } = false;
}
