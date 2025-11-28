namespace BusinessLogic;


public class AssignmentStarters 
{
    /// 1. Check if a book ID is a power of two
    public bool IsPowerOfTwo(int bookId)
    {
        if (bookId <= 0)
            return false;

        var rest = bookId % 2;

        return rest == 0;
    }

    /// 2. Reverse a book title 
    public string ReverseTitle(string title)
    {
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
