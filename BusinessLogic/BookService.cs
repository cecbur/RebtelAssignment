namespace BusinessLogic;

/// <summary>
/// Implementation of book-related business operations following Clean Code and SOLID principles
/// </summary>
public class BookService : IBookService
{
    /// <summary>
    /// Checks if a given Book ID is a power of two
    /// A number is a power of two if it has exactly one bit set in its binary representation
    /// </summary>
    /// <param name="bookId">The Book ID to check</param>
    /// <returns>True if the Book ID is a power of two, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when bookId is less than or equal to zero</exception>
    public bool IsPowerOfTwo(int bookId)
    {
        if (bookId <= 0)
        {
            throw new ArgumentException("Book ID must be a positive integer", nameof(bookId));
        }

        // A power of two has exactly one bit set
        // Using bitwise AND operation: n & (n-1) equals 0 for powers of two
        // Examples: 8 (1000) & 7 (0111) = 0, but 6 (0110) & 5 (0101) = 4
        return (bookId & (bookId - 1)) == 0;
    }

    /// <summary>
    /// Reverses a book title string
    /// </summary>
    /// <param name="title">The book title to reverse</param>
    /// <returns>The reversed book title</returns>
    /// <exception cref="ArgumentNullException">Thrown when title is null</exception>
    public string ReverseTitle(string title)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title), "Title cannot be null");
        }

        // Convert to char array, reverse, and convert back to string
        char[] charArray = title.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Generates a string by repeating a book title a specified number of times
    /// </summary>
    /// <param name="title">The book title to repeat</param>
    /// <param name="count">The number of times to repeat the title</param>
    /// <returns>The title repeated count times</returns>
    /// <exception cref="ArgumentNullException">Thrown when title is null</exception>
    /// <exception cref="ArgumentException">Thrown when count is negative</exception>
    public string GenerateTitleReplicas(string title, int count)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title), "Title cannot be null");
        }

        if (count < 0)
        {
            throw new ArgumentException("Count must be non-negative", nameof(count));
        }

        if (count == 0)
        {
            return string.Empty;
        }

        // Use string.Create for better performance with large strings
        return string.Concat(Enumerable.Repeat(title, count));
    }

    /// <summary>
    /// Lists all odd-numbered Book IDs from a collection
    /// </summary>
    /// <param name="bookIds">Collection of book IDs to filter</param>
    /// <returns>Collection containing only odd-numbered book IDs</returns>
    /// <exception cref="ArgumentNullException">Thrown when bookIds is null</exception>
    public IEnumerable<int> GetOddNumberedBookIds(IEnumerable<int> bookIds)
    {
        if (bookIds == null)
        {
            throw new ArgumentNullException(nameof(bookIds), "Book IDs collection cannot be null");
        }

        // Use LINQ for clean, declarative filtering
        // Modulo operator determines if number is odd
        return bookIds.Where(id => id % 2 != 0);
    }
}
