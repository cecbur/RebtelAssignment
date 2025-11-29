using BusinessLogic;
using BusinessLogic.Grpc;
using BusinessLogicContracts;
using BusinessLogicContracts.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace BusinessLogicGrpcClient;

public class BusinessLogicGrpcFacade : IBusinessLogicFacade
{
    private readonly GrpcChannel _channel;
    private readonly BusinessLogicService.BusinessLogicServiceClient _client;

    public BusinessLogicGrpcFacade(string serverAddress)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = new BusinessLogicService.BusinessLogicServiceClient(_channel);
    }

    public async Task<BusinessLogicContracts.Dto.BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null)
    {
        var request = new GetBooksSortedByMostLoanedRequest();
        if (maxBooksToReturn.HasValue)
        {
            request.MaxBooksToReturn = maxBooksToReturn.Value;
        }

        var response = await _client.GetBooksSortedByMostLoanedAsync(request);
        return response.BookLoans.Select(MapFromGrpcBookLoans).ToArray();
    }

    public async Task<BusinessLogicContracts.Dto.PatronLoans[]> GetPatronsOrderedByLoanFrequency(DateTime startDate, DateTime endDate)
    {
        var request = new GetPatronsOrderedByLoanFrequencyRequest
        {
            StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(startDate, DateTimeKind.Utc)),
            EndDate = Timestamp.FromDateTime(DateTime.SpecifyKind(endDate, DateTimeKind.Utc))
        };

        var response = await _client.GetPatronsOrderedByLoanFrequencyAsync(request);
        return response.PatronLoans.Select(MapFromGrpcPatronLoans).ToArray();
    }

    public async Task<double?> GetPagesPerDay(int loanId)
    {
        var request = new GetPagesPerDayRequest { LoanId = loanId };
        var response = await _client.GetPagesPerDayAsync(request);
        return response.PagesPerDay;
    }

    public async Task<BusinessLogicContracts.Dto.BookFrequency[]> GetOtherBooksBorrowed(int bookId)
    {
        var request = new GetOtherBooksBorrowedRequest { BookId = bookId };
        var response = await _client.GetOtherBooksBorrowedAsync(request);
        return response.BookFrequencies.Select(MapFromGrpcBookFrequency).ToArray();
    }

    // Mapping methods
    private static BusinessLogicContracts.Dto.BookLoans MapFromGrpcBookLoans(BusinessLogic.Grpc.BookLoans grpcBookLoans)
    {
        return new BusinessLogicContracts.Dto.BookLoans
        {
            Book = MapFromGrpcBook(grpcBookLoans.Book),
            LoanCount = grpcBookLoans.LoanCount
        };
    }

    private static BusinessLogicContracts.Dto.PatronLoans MapFromGrpcPatronLoans(BusinessLogic.Grpc.PatronLoans grpcPatronLoans)
    {
        return new BusinessLogicContracts.Dto.PatronLoans(
            MapFromGrpcPatron(grpcPatronLoans.Patron),
            grpcPatronLoans.Loans.Select(MapFromGrpcLoan).ToArray()
        );
    }

    private static BusinessLogicContracts.Dto.BookFrequency MapFromGrpcBookFrequency(BusinessLogic.Grpc.BookFrequency grpcBookFrequency)
    {
        return new BusinessLogicContracts.Dto.BookFrequency
        {
            AssociatedBook = MapFromGrpcBook(grpcBookFrequency.AssociatedBook),
            LoansOfThisBookPerLoansOfMainBook = grpcBookFrequency.LoansOfThisBookPerLoansOfMainBook
        };
    }

    private static BusinessModels.Book MapFromGrpcBook(BusinessLogic.Grpc.Book grpcBook)
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

    private static BusinessModels.Patron MapFromGrpcPatron(BusinessLogic.Grpc.Patron grpcPatron)
    {
        return new BusinessModels.Patron
        {
            Id = grpcPatron.Id,
            FirstName = grpcPatron.FirstName,
            LastName = grpcPatron.LastName,
            Email = grpcPatron.Email,
            PhoneNumber = grpcPatron.PhoneNumber,
            MembershipDate = grpcPatron.MembershipDate.ToDateTime(),
            IsActive = grpcPatron.IsActive
        };
    }

    private static BusinessModels.Loan MapFromGrpcLoan(BusinessLogic.Grpc.Loan grpcLoan)
    {
        var loan = new BusinessModels.Loan
        {
            Id = grpcLoan.Id,
            Book = MapFromGrpcBook(grpcLoan.Book),
            Patron = MapFromGrpcPatron(grpcLoan.Patron),
            LoanDate = grpcLoan.LoanDate.ToDateTime(),
            DueDate = grpcLoan.DueDate.ToDateTime(),
            ReturnDate = grpcLoan.ReturnDate?.ToDateTime(),
            IsReturned = grpcLoan.IsReturned
        };

        return loan;
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}