namespace BusinessModels;

public class Patron
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime MembershipDate { get; set; }

    public bool IsActive { get; set; } = true;
    
    public override bool Equals(object obj)
    {
        var item = obj as Patron;

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
