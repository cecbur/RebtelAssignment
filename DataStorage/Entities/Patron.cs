namespace DataStorage.Entities;

public class Patron
{
    public int PatronId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime MembershipDate { get; set; }

    public bool IsActive { get; set; } = true;
}
