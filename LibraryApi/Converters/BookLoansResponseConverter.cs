using BusinessLogicContracts.Dto;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for BookLoans to BookLoansResponse
/// </summary>
public static class BookLoansResponseConverter
{
    /// <summary>
    /// Converts a BookLoans to BookLoansResponse DTO
    /// </summary>
    public static BookLoansResponse ToDto(BookLoans bookLoans)
    {
        return new BookLoansResponse
        {
            Book = BookDtoConverter.ToDto(bookLoans.Book),
            LoanCount = bookLoans.LoanCount
        };
    }

    /// <summary>
    /// Converts a collection of BookLoans to BookLoansResponse DTOs
    /// </summary>
    public static BookLoansResponse[] ToDto(IEnumerable<BookLoans> bookLoans)
    {
        return bookLoans.Select(ToDto).ToArray();
    }
}
