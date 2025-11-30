using BusinessModels;
using LibraryApi.DTOs;

namespace LibraryApi.Converters;

/// <summary>
/// Converter for Book entity to BookDto
/// </summary>
public static class BookDtoConverter
{
    /// <summary>
    /// Converts a Book business model to BookDto
    /// </summary>
    public static BookDto ToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = AuthorDtoConverter.ToDto(book.Author),
            ISBN = book.Isbn,
            PublicationYear = book.PublicationYear,
            NumberOfPages = book.NumberOfPages,
            IsAvailable = book.IsAvailableForLoan
        };
    }

    /// <summary>
    /// Converts a BookDto to Book business model
    /// </summary>
    public static Book FromDto(BookDto bookDto)
    {
        return new Book
        {
            Id = bookDto.Id,
            Title = bookDto.Title,
            Author = AuthorDtoConverter.FromDto(bookDto.Author),
            Isbn = bookDto.ISBN,
            PublicationYear = bookDto.PublicationYear,
            NumberOfPages = bookDto.NumberOfPages,
            IsAvailableForLoan = bookDto.IsAvailable
        };
    }
}
