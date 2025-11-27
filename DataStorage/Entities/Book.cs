namespace DataStorage.Entities;

/// <summary>
/// Represents a book entity in the library database
/// Plain Old CLR Object (POCO) - no ORM dependencies
/// </summary>
public class Book
{
    /// <summary>
    /// Unique identifier for the book
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Title of the book
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Author of the book
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// ISBN of the book
    /// </summary>
    public string? ISBN { get; set; }

    /// <summary>
    /// Year the book was published
    /// </summary>
    public int? PublicationYear { get; set; }

    /// <summary>
    /// Indicates whether the book is currently available for borrowing
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
