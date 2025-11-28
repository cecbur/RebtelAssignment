using BusinessLogic;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for BookPatterns.BookLoans to BookLoansResponse
/// </summary>
public static class BookLoansResponseConverter
{
    /// <summary>
    /// Converts a BookPatterns.BookLoans to BookLoansResponse DTO
    /// </summary>
    public static BookLoansResponse ToDto(BookPatterns.BookLoans bookLoans)
    {
        return new BookLoansResponse
        {
            Book = BookDtoConverter.ToDto(bookLoans.Book),
            LoanCount = bookLoans.LoanCount
        };
    }

    /// <summary>
    /// Converts a collection of BookPatterns.BookLoans to BookLoansResponse DTOs
    /// </summary>
    public static BookLoansResponse[] ToDto(IEnumerable<BookPatterns.BookLoans> bookLoans)
    {
        return bookLoans.Select(ToDto).ToArray();
    }
}
