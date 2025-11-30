using BusinessLogicContracts.Dto;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for BookFrequency to BookFrequencyResponse
/// </summary>
public static class BookFrequencyResponseConverter
{
    /// <summary>
    /// Converts a BookFrequency to BookFrequencyResponse DTO
    /// </summary>
    public static BookFrequencyResponse ToDto(BookFrequency bookFrequency)
    {
        return new BookFrequencyResponse
        {
            AssociatedBook = BookDtoConverter.ToDto(bookFrequency.AssociatedBook),
            LoansOfThisBookPerLoansOfMainBook = bookFrequency.LoansOfThisBookPerLoansOfMainBook
        };
    }

    /// <summary>
    /// Converts a collection of BookFrequency to BookFrequencyResponse DTOs
    /// </summary>
    public static BookFrequencyResponse[] ToDto(IEnumerable<BookFrequency> bookFrequencies)
    {
        return bookFrequencies.Select(ToDto).ToArray();
    }
}
