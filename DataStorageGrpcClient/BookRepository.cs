using BusinessModels;
using DataStorage.Grpc;
using DataStorageContracts;
using Grpc.Core;
using Grpc.Net.Client;

namespace DataStorageGrpcClient;

public class BookRepository : IBookRepository
{
    private readonly GrpcChannel _channel;
    private readonly BookService.BookServiceClient _client;

    public BookRepository(string serverAddress)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = new BookService.BookServiceClient(_channel);
    }

    public async Task<IEnumerable<BusinessModels.Book>> GetAllBooks()
    {
        var request = new GetAllBooksRequest();
        var response = await _client.GetAllBooksAsync(request);
        return response.Books.Select(MapFromGrpcBook);
    }

    public async Task<BusinessModels.Book> GetBookById(int bookId)
    {
        try
        {
            var request = new GetBookByIdRequest { BookId = bookId };
            var response = await _client.GetBookByIdAsync(request);
            return MapFromGrpcBook(response.Book);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Book with id {bookId} not found", ex);
        }
    }

    public async Task<IEnumerable<BusinessModels.Book>> GetBooksByIds(IEnumerable<int> bookIds)
    {
        var request = new GetBooksByIdsRequest();
        request.BookIds.AddRange(bookIds);
        var response = await _client.GetBooksByIdsAsync(request);
        return response.Books.Select(MapFromGrpcBook);
    }

    public async Task<BusinessModels.Book> AddBook(BusinessModels.Book book)
    {
        var request = new AddBookRequest
        {
            Title = book.Title,
            AuthorId = book.Author?.Id ?? 0,
            Isbn = book.Isbn ?? string.Empty,
            PublicationYear = book.PublicationYear,
            NumberOfPages = book.NumberOfPages,
            IsAvailable = book.IsAvailableForLoan
        };

        var response = await _client.AddBookAsync(request);
        return MapFromGrpcBook(response.Book);
    }

    public async Task<BusinessModels.Book> UpdateBook(BusinessModels.Book book)
    {
        try
        {
            var request = new UpdateBookRequest
            {
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.Author?.Id ?? 0,
                Isbn = book.Isbn ?? string.Empty,
                PublicationYear = book.PublicationYear,
                NumberOfPages = book.NumberOfPages,
                IsAvailable = book.IsAvailableForLoan
            };

            var response = await _client.UpdateBookAsync(request);
            return MapFromGrpcBook(response.Book);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Book with id {book.Id} not found", ex);
        }
    }

    public async Task<bool> DeleteBook(int bookId)
    {
        var request = new DeleteBookRequest { BookId = bookId };
        var response = await _client.DeleteBookAsync(request);
        return response.Success;
    }

    public async Task<IEnumerable<BusinessModels.Book>> SearchBooksByTitleLikeQuery(string titlePattern)
    {
        var request = new SearchBooksByTitleLikeQueryRequest { TitlePattern = titlePattern };
        var response = await _client.SearchBooksByTitleLikeQueryAsync(request);
        return response.Books.Select(MapFromGrpcBook);
    }

    private static BusinessModels.Book MapFromGrpcBook(DataStorage.Grpc.Book grpcBook)
    {
        var book = new BusinessModels.Book
        {
            Id = grpcBook.Id,
            Title = grpcBook.Title,
            Isbn = string.IsNullOrEmpty(grpcBook.Isbn) ? null : grpcBook.Isbn,
            PublicationYear = grpcBook.PublicationYear,
            NumberOfPages = grpcBook.NumberOfPages,
            IsAvailableForLoan = grpcBook.IsAvailable
        };

        if (grpcBook.Author != null)
        {
            book.Author = new BusinessModels.Author
            {
                Id = grpcBook.Author.Id,
                GivenName = grpcBook.Author.GivenName,
                Surname = grpcBook.Author.Surname
            };
        }

        return book;
    }
}
