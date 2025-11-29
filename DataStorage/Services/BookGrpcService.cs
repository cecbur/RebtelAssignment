using Grpc.Core;
using DataStorage.Grpc;
using DataStorage.Repositories;
using Microsoft.Extensions.Logging;

namespace DataStorage.Services;

public class BookGrpcService : BookService.BookServiceBase
{
    private readonly BookRepository _bookRepository;
    private readonly ILogger<BookGrpcService> _logger;

    public BookGrpcService(BookRepository bookRepository, ILogger<BookGrpcService> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetAllBooksResponse> GetAllBooks(GetAllBooksRequest request, ServerCallContext context)
    {
        try
        {
            var books = await _bookRepository.GetAllBooks();
            var response = new GetAllBooksResponse();
            response.Books.AddRange(books.Select(MapToGrpcBook));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all books");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting all books"));
        }
    }

    public override async Task<BookResponse> GetBookById(GetBookByIdRequest request, ServerCallContext context)
    {
        try
        {
            var book = await _bookRepository.GetBookById(request.BookId);
            return new BookResponse { Book = MapToGrpcBook(book) };
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Book with id {request.BookId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book by id {BookId}", request.BookId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting book"));
        }
    }

    public override async Task<GetBooksResponse> GetBooksByIds(GetBooksByIdsRequest request, ServerCallContext context)
    {
        try
        {
            var books = await _bookRepository.GetBooksByIds(request.BookIds);
            var response = new GetBooksResponse();
            response.Books.AddRange(books.Select(MapToGrpcBook));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting books by ids");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting books by ids"));
        }
    }

    public override async Task<BookResponse> AddBook(AddBookRequest request, ServerCallContext context)
    {
        try
        {
            var book = new BusinessModels.Book
            {
                Title = request.Title,
                Author = new BusinessModels.Author { Id = request.AuthorId },
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear,
                NumberOfPages = request.NumberOfPages,
                IsAvailableForLoan = request.IsAvailable
            };

            var addedBook = await _bookRepository.AddBook(book);
            return new BookResponse { Book = MapToGrpcBook(addedBook) };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book");
            throw new RpcException(new Status(StatusCode.Internal, "Error adding book"));
        }
    }

    public override async Task<BookResponse> UpdateBook(UpdateBookRequest request, ServerCallContext context)
    {
        try
        {
            var book = new BusinessModels.Book
            {
                Id = request.Id,
                Title = request.Title,
                Author = new BusinessModels.Author { Id = request.AuthorId },
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear,
                NumberOfPages = request.NumberOfPages,
                IsAvailableForLoan = request.IsAvailable
            };

            var updatedBook = await _bookRepository.UpdateBook(book);
            return new BookResponse { Book = MapToGrpcBook(updatedBook) };
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Book with id {request.Id} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book");
            throw new RpcException(new Status(StatusCode.Internal, "Error updating book"));
        }
    }

    public override async Task<DeleteBookResponse> DeleteBook(DeleteBookRequest request, ServerCallContext context)
    {
        try
        {
            var success = await _bookRepository.DeleteBook(request.BookId);
            return new DeleteBookResponse { Success = success };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book");
            throw new RpcException(new Status(StatusCode.Internal, "Error deleting book"));
        }
    }

    public override async Task<GetBooksResponse> SearchBooksByTitleLikeQuery(SearchBooksByTitleLikeQueryRequest request, ServerCallContext context)
    {
        try
        {
            var books = await _bookRepository.SearchBooksByTitleLikeQuery(request.TitlePattern);
            var response = new GetBooksResponse();
            response.Books.AddRange(books.Select(MapToGrpcBook));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching books by title pattern {TitlePattern}", request.TitlePattern);
            throw new RpcException(new Status(StatusCode.Internal, "Error searching books"));
        }
    }

    private static Book MapToGrpcBook(BusinessModels.Book book)
    {
        var grpcBook = new Book
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn ?? string.Empty,
            PublicationYear = book.PublicationYear,
            NumberOfPages = book.NumberOfPages,
            IsAvailable = book.IsAvailableForLoan
        };

        if (book.Author != null)
        {
            grpcBook.Author = new Author
            {
                Id = book.Author.Id,
                GivenName = book.Author.GivenName,
                Surname = book.Author.Surname,
                Name = book.Author.Name
            };
        }

        return grpcBook;
    }
}
