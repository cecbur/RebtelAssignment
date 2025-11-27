namespace DataStorage.Exceptions;

public class BookIdMissingException : Exception
{
    public int BookId { get; set; }
    public BookIdMissingException(string message, int bookId) : base(message) {} 
}