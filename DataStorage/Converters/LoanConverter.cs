using DataStorage.Entities;
using BusinessModels;

namespace DataStorage.Converters;

internal static class LoanConverter
{
    public static BusinessModels.Loan ToModel(Entities.Loan entity)
    {
        return new BusinessModels.Loan
        {
            Id = entity.Id,
            BookId = entity.BookId,
            PatronId = entity.PatronId,
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
            BookId = model.BookId,
            PatronId = model.PatronId,
            LoanDate = model.LoanDate,
            DueDate = model.DueDate,
            ReturnDate = model.ReturnDate,
            IsReturned = model.IsReturned
        };
    }
}
