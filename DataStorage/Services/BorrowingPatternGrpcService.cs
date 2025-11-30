using Grpc.Core;
using DataStorage.Grpc;
using DataStorage.RepositoriesMultipleTables;
using DataStorageContracts;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace DataStorage.Services;

public class BorrowingPatternGrpcService : BorrowingPatternService.BorrowingPatternServiceBase
{
    private readonly BorrowingPatternRepository _borrowingPatternRepository;
    private readonly ILogger<BorrowingPatternGrpcService> _logger;

    public BorrowingPatternGrpcService(
        BorrowingPatternRepository borrowingPatternRepository,
        ILogger<BorrowingPatternGrpcService> logger)
    {
        _borrowingPatternRepository = borrowingPatternRepository ?? throw new ArgumentNullException(nameof(borrowingPatternRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetOtherBooksBorrowedResponse> GetOtherBooksBorrowed(
        GetOtherBooksBorrowedRequest request,
        ServerCallContext context)
    {
        try
        {
            var associatedBooks = await _borrowingPatternRepository.GetOtherBooksBorrowed(request.BookId);
            var response = new GetOtherBooksBorrowedResponse()
            {
                AssociatedBooks = MapToGrpcAssociatedBooks(associatedBooks)
            };
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting other books borrowed for book id {BookId}", request.BookId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting other books borrowed"));
        }
    }

    private static AssociatedBooks MapToGrpcAssociatedBooks(DataStorageContracts.Dto.AssociatedBooks associatedBooks)
    {
        var grpcAssociatedBooks = new AssociatedBooks
        {
            Book = MapToGrpcBook(associatedBooks.Book)
        };

        grpcAssociatedBooks.Associated.AddRange(
            associatedBooks.Associated.Select(bc => new BookCount
            {
                Book = MapToGrpcBook(bc.Book),
                Count = bc.Count
            })
        );

        return grpcAssociatedBooks;
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
