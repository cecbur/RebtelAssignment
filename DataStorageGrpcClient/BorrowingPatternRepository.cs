using BusinessModels;
using DataStorage.Grpc;
using DataStorageContracts;
using Grpc.Core;
using Grpc.Net.Client;

namespace DataStorageGrpcClient;

public class BorrowingPatternRepository : DataStorageContracts.IBorrowingPatternRepository
{
    private readonly GrpcChannel _channel;
    private readonly DataStorage.Grpc.BorrowingPatternService.BorrowingPatternServiceClient _client;

    public BorrowingPatternRepository(string serverAddress)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = new DataStorage.Grpc.BorrowingPatternService.BorrowingPatternServiceClient(_channel);
    }

    public async Task<DataStorageContracts.Dto.AssociatedBooks> GetOtherBooksBorrowed(int bookId)
    {
        try
        {
            var request = new DataStorage.Grpc.GetOtherBooksBorrowedRequest { BookId = bookId };
            var response = await _client.GetOtherBooksBorrowedAsync(request);
            return MapFromGrpcAssociatedBooks(response.AssociatedBooks);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException($"Error getting other books borrowed for book id {bookId}", ex);
        }
    }

    private static DataStorageContracts.Dto.AssociatedBooks MapFromGrpcAssociatedBooks(DataStorage.Grpc.AssociatedBooks grpcAssociatedBooks)
    {
        return new DataStorageContracts.Dto.AssociatedBooks
        {
            Book = MapFromGrpcBook(grpcAssociatedBooks.Book),
            Associated = grpcAssociatedBooks.Associated.Select(bc => new DataStorageContracts.Dto.AssociatedBooks.BookCount
            {
                Book = MapFromGrpcBook(bc.Book),
                Count = bc.Count
            }).ToArray()
        };
    }

    private static BusinessModels.Book MapFromGrpcBook(DataStorage.Grpc.Book grpcBook)
    {
        var book = new BusinessModels.Book
        {
            Id = grpcBook.Id,
            Title = grpcBook.Title,
            Isbn = grpcBook.Isbn,
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

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
