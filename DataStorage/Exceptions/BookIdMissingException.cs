namespace DataStorage.Exceptions;

public class BookIdMissingException : Exception
{
    public int BookId { get; set; }

    public BookIdMissingException(string message, int bookId) : base(message)
    {
        Init(bookId);
    } 
    
    public BookIdMissingException(string message, int bookId, Exception e) : base(message, e)
    {
        Init(bookId);
    } 
    
    private void Init(int bookId)
    {
        BookId = bookId;
    } 
    
}