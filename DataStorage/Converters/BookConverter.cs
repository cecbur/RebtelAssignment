using DataStorage.Entities;
using BusinessModels;

namespace DataStorage.Converters;

internal static class BookConverter
{
    public static BusinessModels.Book ToModel(Entities.Book entity, Entities.Author? author)
    {
        return new BusinessModels.Book
        {
            BookId = entity.BookId,
            Title = entity.Title,
            Author = author != null ? AuthorConverter.ToModel(author) : null,
            Isbn = entity.Isbn,
            PublicationYear = entity.PublicationYear,
            NumberOfPages = entity.NumberOfPages,
            IsAvailableForLoan = entity.IsAvailableForLoan
        };
    }

    public static Entities.Book ToEntity(BusinessModels.Book model)
    {
        return new Entities.Book
        {
            BookId = model.BookId,
            Title = model.Title,
            AuthorId = model.Author?.AuthorId,
            Isbn = model.Isbn,
            PublicationYear = model.PublicationYear,
            NumberOfPages = model.NumberOfPages,
            IsAvailableForLoan = model.IsAvailableForLoan
        };
    }
}
