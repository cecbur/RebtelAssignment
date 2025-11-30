namespace BusinessModels;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public Author? Author { get; set; }

    public string? Isbn { get; set; }

    public int? PublicationYear { get; set; }

    public int? NumberOfPages { get; set; }

    public bool IsAvailableForLoan { get; set; } = true;
    
    public override bool Equals(object? obj)
    {
        var item = obj as Book;

        if (item == null)
        {
            return false;
        }

        return this.Id.Equals(item.Id);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode    // Id is the identity. If Id changes, it is correct that the identity changes.
        return this.Id.GetHashCode();
    }

}
