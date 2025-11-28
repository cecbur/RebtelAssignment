namespace LibraryApi.DTOs;

/// <summary>
/// Data Transfer Object for Patron information
/// </summary>
public class PatronDto
{
    /// <summary>
    /// Unique identifier for the patron
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Patron's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Patron's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Patron's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Patron's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date when the patron became a member
    /// </summary>
    public DateTime MembershipDate { get; set; }

    /// <summary>
    /// Indicates if the patron's membership is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
