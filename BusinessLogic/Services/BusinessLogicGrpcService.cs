using Grpc.Core;
using BusinessLogic.Grpc;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class BusinessLogicGrpcService : BusinessLogicService.BusinessLogicServiceBase
{
    private readonly Facade _facade;
    private readonly ILogger<BusinessLogicGrpcService> _logger;

    public BusinessLogicGrpcService(Facade facade, ILogger<BusinessLogicGrpcService> logger)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetBooksSortedByMostLoanedResponse> GetBooksSortedByMostLoaned(
        GetBooksSortedByMostLoanedRequest request,
        ServerCallContext context)
    {
        try
        {
            var maxBooks = request.MaxBooksToReturn;
            var bookLoans = await _facade.GetBooksSortedByMostLoaned(maxBooks);
            var response = new GetBooksSortedByMostLoanedResponse();
            response.BookLoans.AddRange(bookLoans.Select(MapToGrpcBookLoans));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting books sorted by most loaned");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting books sorted by most loaned"));
        }
    }

    public override async Task<GetPatronsOrderedByLoanFrequencyResponse> GetPatronsOrderedByLoanFrequency(
        GetPatronsOrderedByLoanFrequencyRequest request,
        ServerCallContext context)
    {
        try
        {
            var startDate = request.StartDate.ToDateTime();
            var endDate = request.EndDate.ToDateTime();
            var patronLoans = await _facade.GetPatronsOrderedByLoanFrequency(startDate, endDate);
            var response = new GetPatronsOrderedByLoanFrequencyResponse();
            response.PatronLoans.AddRange(patronLoans.Select(MapToGrpcPatronLoans));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patrons ordered by loan frequency");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting patrons ordered by loan frequency"));
        }
    }

    public override async Task<GetPagesPerDayResponse> GetPagesPerDay(
        GetPagesPerDayRequest request,
        ServerCallContext context)
    {
        try
        {
            var pagesPerDay = await _facade.GetPagesPerDay(request.LoanId);
            var response = new GetPagesPerDayResponse();
            if (pagesPerDay.HasValue)
            {
                response.PagesPerDay = pagesPerDay.Value;
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pages per day for loan {LoanId}", request.LoanId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting pages per day"));
        }
    }

    public override async Task<GetOtherBooksBorrowedResponse> GetOtherBooksBorrowed(
        GetOtherBooksBorrowedRequest request,
        ServerCallContext context)
    {
        try
        {
            var bookFrequencies = await _facade.GetOtherBooksBorrowed(request.BookId);
            var response = new GetOtherBooksBorrowedResponse();
            response.BookFrequencies.AddRange(bookFrequencies.Select(MapToGrpcBookFrequency));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting other books borrowed for book {BookId}", request.BookId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting other books borrowed"));
        }
    }

    // Mapping methods
    private static BookLoans MapToGrpcBookLoans(BusinessLogicContracts.Dto.BookLoans bookLoans)
    {
        return new BookLoans
        {
            Book = MapToGrpcBook(bookLoans.Book),
            LoanCount = bookLoans.LoanCount
        };
    }

    private static PatronLoans MapToGrpcPatronLoans(BusinessLogicContracts.Dto.PatronLoans patronLoans)
    {
        var grpcPatronLoans = new PatronLoans
        {
            Patron = MapToGrpcPatron(patronLoans.Patron),
            LoanCount = patronLoans.LoanCount
        };
        grpcPatronLoans.Loans.AddRange(patronLoans.Loans.Select(MapToGrpcLoan));
        return grpcPatronLoans;
    }

    private static BookFrequency MapToGrpcBookFrequency(BusinessLogicContracts.Dto.BookFrequency bookFrequency)
    {
        return new BookFrequency
        {
            AssociatedBook = MapToGrpcBook(bookFrequency.AssociatedBook),
            LoansOfThisBookPerLoansOfMainBook = bookFrequency.LoansOfThisBookPerLoansOfMainBook
        };
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

    private static Patron MapToGrpcPatron(BusinessModels.Patron patron)
    {
        return new Patron
        {
            Id = patron.Id,
            FirstName = patron.FirstName,
            LastName = patron.LastName,
            Email = patron.Email,
            PhoneNumber = patron.PhoneNumber,
            MembershipDate = Timestamp.FromDateTime(DateTime.SpecifyKind(patron.MembershipDate, DateTimeKind.Utc)),
            IsActive = patron.IsActive
        };
    }

    private static Loan MapToGrpcLoan(BusinessModels.Loan loan)
    {
        var grpcLoan = new Loan
        {
            Id = loan.Id,
            LoanDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.LoanDate, DateTimeKind.Utc)),
            DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.DueDate, DateTimeKind.Utc)),
            IsReturned = loan.IsReturned
        };

        if (loan.ReturnDate.HasValue)
        {
            grpcLoan.ReturnDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.ReturnDate.Value, DateTimeKind.Utc));
        }

        if (loan.Book != null)
        {
            grpcLoan.Book = MapToGrpcBook(loan.Book);
        }

        if (loan.Patron != null)
        {
            grpcLoan.Patron = MapToGrpcPatron(loan.Patron);
        }

        return grpcLoan;
    }
}
