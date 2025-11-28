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
        char[] charArray = title.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// 3. Generates book title replicas
    public string GenerateTitleRepetitions(string title, int count)
    {
        var repeated = "";
        for (int i = 0; i < count; i++)
        {
            repeated += title;
        }

        return repeated;
    }

    /// 4. Lists odd-numbered Book IDs 
    public void PrintOddNumberedBookIds0To100()
    {
        var ids = Enumerable.Range(0, 100).ToArray();
        var odd = ids.Where(id => id % 2 != 0);
        Console.WriteLine(string.Join(Environment.NewLine, odd));
    }

}
