using DataStorage.Entities;
using BusinessModels;

namespace DataStorage.Converters;

internal static class LoanConverter
{
    public static BusinessModels.Loan ToModel(Entities.Loan entity, Entities.Book? book, Entities.Author? author, Entities.Patron? patron)
    {
        return new BusinessModels.Loan
        {
            Id = entity.Id,
            Book = book != null ? BookConverter.ToModel(book, author) : null,
            Patron = patron != null ? PatronConverter.ToModel(patron) : null,
            LoanDate = entity.LoanDate,
            DueDate = entity.DueDate,
            ReturnDate = entity.ReturnDate,
            IsReturned = entity.IsReturned
        };
    }

    public static Entities.Loan ToEntity(BusinessModels.Loan model)
    {
        return new Entities.Loan
        {
            Id = model.Id,
            BookId = model.Book?.Id ?? 0,
            PatronId = model.Patron?.Id ?? 0,
            LoanDate = model.LoanDate,
            DueDate = model.DueDate,
            ReturnDate = model.ReturnDate,
            IsReturned = model.IsReturned
        };
    }
}
