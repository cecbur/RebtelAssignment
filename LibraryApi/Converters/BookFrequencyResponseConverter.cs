using BusinessLogic;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for BorrowingPatterns.BookFrequency to BookFrequencyResponse
/// </summary>
public static class BookFrequencyResponseConverter
{
    /// <summary>
    /// Converts a BorrowingPatterns.BookFrequency to BookFrequencyResponse DTO
    /// </summary>
    public static BookFrequencyResponse ToDto(BorrowingPatterns.BookFrequency bookFrequency)
    {
        return new BookFrequencyResponse
        {
            AssociatedBook = BookDtoConverter.ToDto(bookFrequency.AssociatedBook),
            LoansOfThisBookPerLoansOfMainBook = bookFrequency.LoansOfThisBookPerLoansOfMainBook
        };
    }

    /// <summary>
    /// Converts a collection of BorrowingPatterns.BookFrequency to BookFrequencyResponse DTOs
    /// </summary>
    public static BookFrequencyResponse[] ToDto(IEnumerable<BorrowingPatterns.BookFrequency> bookFrequencies)
    {
        return bookFrequencies.Select(ToDto).ToArray();
    }
}
