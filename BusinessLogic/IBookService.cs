namespace BusinessLogic;

/// <summary>
/// Interface defining book-related business operations
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Checks if a given Book ID is a power of two
    /// </summary>
    /// <param name="bookId">The Book ID to check</param>
    /// <returns>True if the Book ID is a power of two, false otherwise</returns>
    bool IsPowerOfTwo(int bookId);

    /// <summary>
    /// Reverses a book title string
    /// </summary>
    /// <param name="title">The book title to reverse</param>
    /// <returns>The reversed book title</returns>
    string ReverseTitle(string title);

    /// <summary>
    /// Generates a string by repeating a book title a specified number of times
    /// </summary>
    /// <param name="title">The book title to repeat</param>
    /// <param name="count">The number of times to repeat the title</param>
    /// <returns>The title repeated count times</returns>
    string GenerateTitleReplicas(string title, int count);

    /// <summary>
    /// Lists all odd-numbered Book IDs from a collection
    /// </summary>
    /// <param name="bookIds">Collection of book IDs to filter</param>
    /// <returns>Collection containing only odd-numbered book IDs</returns>
    IEnumerable<int> GetOddNumberedBookIds(IEnumerable<int> bookIds);
}
