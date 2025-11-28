using BusinessModels;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for Author entity to AuthorDto
/// </summary>
public static class AuthorDtoConverter
{
    /// <summary>
    /// Converts an Author business model to AuthorDto
    /// </summary>
    public static AuthorDto? ToDto(Author? author)
    {
        if (author == null)
            return null;

        return new AuthorDto
        {
            Id = author.Id,
            GivenName = author.GivenName,
            Surname = author.Surname
        };
    }

    /// <summary>
    /// Converts an AuthorDto to Author business model
    /// </summary>
    public static Author? FromDto(AuthorDto? authorDto)
    {
        if (authorDto == null)
            return null;

        return new Author
        {
            Id = authorDto.Id,
            GivenName = authorDto.GivenName,
            Surname = authorDto.Surname
        };
    }
}
