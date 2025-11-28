using BusinessModels;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for Loan to LoanDto
/// </summary>
public static class LoanDtoConverter
{
    /// <summary>
    /// Converts a Loan business model to LoanDto
    /// </summary>
    public static LoanDto ToDto(Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            Book = BookDtoConverter.ToDto(loan.Book),
            Patron = PatronDtoConverter.ToDto(loan.Patron),
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        };
    }

    /// <summary>
    /// Converts a collection of Loan business models to LoanDto collection
    /// </summary>
    public static IEnumerable<LoanDto> ToDto(IEnumerable<Loan> loans)
    {
        return loans.Select(ToDto);
    }

    /// <summary>
    /// Converts a LoanDto to Loan business model
    /// </summary>
    public static Loan FromDto(LoanDto loanDto)
    {
        return new Loan
        {
            Id = loanDto.Id,
            Book = BookDtoConverter.FromDto(loanDto.Book),
            Patron = PatronDtoConverter.FromDto(loanDto.Patron),
            LoanDate = loanDto.LoanDate,
            DueDate = loanDto.DueDate,
            ReturnDate = loanDto.ReturnDate,
            IsReturned = loanDto.IsReturned
        };
    }
}
