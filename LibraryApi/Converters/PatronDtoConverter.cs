using BusinessModels;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for Patron to PatronDto
/// </summary>
public static class PatronDtoConverter
{
    /// <summary>
    /// Converts a Patron business model to PatronDto
    /// </summary>
    public static PatronDto ToDto(Patron patron)
    {
        return new PatronDto
        {
            Id = patron.Id,
            FirstName = patron.FirstName,
            LastName = patron.LastName,
            Email = patron.Email,
            PhoneNumber = patron.PhoneNumber,
            MembershipDate = patron.MembershipDate,
            IsActive = patron.IsActive
        };
    }

    /// <summary>
    /// Converts a PatronDto to Patron business model
    /// </summary>
    public static Patron FromDto(PatronDto patronDto)
    {
        return new Patron
        {
            Id = patronDto.Id,
            FirstName = patronDto.FirstName,
            LastName = patronDto.LastName,
            Email = patronDto.Email,
            PhoneNumber = patronDto.PhoneNumber,
            MembershipDate = patronDto.MembershipDate,
            IsActive = patronDto.IsActive
        };
    }
}
