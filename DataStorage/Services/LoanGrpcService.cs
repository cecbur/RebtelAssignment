using Grpc.Core;
using DataStorage.Grpc;
using DataStorage.Repositories;
using DataStorageContracts;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace DataStorage.Services;

public class LoanGrpcService : LoanService.LoanServiceBase
{
    private readonly LoanRepository _loanRepository;
    private readonly ILogger<LoanGrpcService> _logger;

    public LoanGrpcService(LoanRepository loanRepository, ILogger<LoanGrpcService> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetAllLoansResponse> GetAllLoans(GetAllLoansRequest request, ServerCallContext context)
    {
        try
        {
            var loans = await _loanRepository.GetAllLoans();
            var response = new GetAllLoansResponse();
            response.Loans.AddRange(loans.Select(MapToGrpcLoan));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all loans");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting all loans"));
        }
    }

    public override async Task<LoanResponse> GetLoanById(GetLoanByIdRequest request, ServerCallContext context)
    {
        try
        {
            var loan = await _loanRepository.GetLoanById(request.LoanId);
            return new LoanResponse { Loan = MapToGrpcLoan(loan) };
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Loan with id {request.LoanId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loan by id {LoanId}", request.LoanId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting loan"));
        }
    }

    public override async Task<GetLoansResponse> GetLoansByPatronId(GetLoansByPatronIdRequest request, ServerCallContext context)
    {
        try
        {
            var loans = await _loanRepository.GetLoansByPatronId(request.PatronId);
            var response = new GetLoansResponse();
            response.Loans.AddRange(loans.Select(MapToGrpcLoan));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loans by patron id {PatronId}", request.PatronId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting loans by patron"));
        }
    }

    public override async Task<GetLoansResponse> GetLoansByBookId(GetLoansByBookIdRequest request, ServerCallContext context)
    {
        try
        {
            var loans = await _loanRepository.GetLoansByBookId(request.BookId);
            var response = new GetLoansResponse();
            response.Loans.AddRange(loans.Select(MapToGrpcLoan));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loans by book id {BookId}", request.BookId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting loans by book"));
        }
    }

    public override async Task<GetLoansResponse> GetActiveLoans(GetActiveLoansRequest request, ServerCallContext context)
    {
        try
        {
            var loans = await _loanRepository.GetActiveLoans();
            var response = new GetLoansResponse();
            response.Loans.AddRange(loans.Select(MapToGrpcLoan));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active loans");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting active loans"));
        }
    }

    public override async Task<GetLoansResponse> GetLoansByTime(GetLoansByTimeRequest request, ServerCallContext context)
    {
        try
        {
            var startDate = request.StartDate.ToDateTime();
            var endDate = request.EndDate.ToDateTime();
            var loans = await _loanRepository.GetLoansByTime(startDate, endDate);
            var response = new GetLoansResponse();
            response.Loans.AddRange(loans.Select(MapToGrpcLoan));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loans by time range {StartDate} to {EndDate}", request.StartDate, request.EndDate);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting loans by time"));
        }
    }

    public override async Task<LoanResponse> AddLoan(AddLoanRequest request, ServerCallContext context)
    {
        try
        {
            var loan = new BusinessModels.Loan
            {
                Book = new BusinessModels.Book { Id = request.BookId },
                Patron = new BusinessModels.Patron { Id = request.PatronId },
                LoanDate = request.LoanDate.ToDateTime(),
                DueDate = request.DueDate.ToDateTime(),
                ReturnDate = request.ReturnDate?.ToDateTime(),
                IsReturned = request.IsReturned
            };

            var addedLoan = await _loanRepository.AddLoan(loan);
            return new LoanResponse { Loan = MapToGrpcLoan(addedLoan) };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding loan");
            throw new RpcException(new Status(StatusCode.Internal, "Error adding loan"));
        }
    }

    public override async Task<LoanResponse> UpdateLoan(UpdateLoanRequest request, ServerCallContext context)
    {
        try
        {
            var loan = new BusinessModels.Loan
            {
                Id = request.Id,
                Book = new BusinessModels.Book { Id = request.BookId },
                Patron = new BusinessModels.Patron { Id = request.PatronId },
                LoanDate = request.LoanDate.ToDateTime(),
                DueDate = request.DueDate.ToDateTime(),
                ReturnDate = request.ReturnDate?.ToDateTime(),
                IsReturned = request.IsReturned
            };

            var updatedLoan = await _loanRepository.UpdateLoan(loan);
            return new LoanResponse { Loan = MapToGrpcLoan(updatedLoan) };
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Loan with id {request.Id} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan");
            throw new RpcException(new Status(StatusCode.Internal, "Error updating loan"));
        }
    }

    public override async Task<DeleteLoanResponse> DeleteLoan(DeleteLoanRequest request, ServerCallContext context)
    {
        try
        {
            var success = await _loanRepository.DeleteLoan(request.LoanId);
            return new DeleteLoanResponse { Success = success };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loan");
            throw new RpcException(new Status(StatusCode.Internal, "Error deleting loan"));
        }
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
            grpcLoan.Book = new Book
            {
                Id = loan.Book.Id,
                Title = loan.Book.Title,
                Isbn = loan.Book.Isbn ?? string.Empty,
                PublicationYear = loan.Book.PublicationYear,
                NumberOfPages = loan.Book.NumberOfPages,
                IsAvailable = loan.Book.IsAvailableForLoan
            };

            if (loan.Book.Author != null)
            {
                grpcLoan.Book.Author = new Author
                {
                    Id = loan.Book.Author.Id,
                    GivenName = loan.Book.Author.GivenName,
                    Surname = loan.Book.Author.Surname,
                    Name = loan.Book.Author.Name
                };
            }
        }

        if (loan.Patron != null)
        {
            grpcLoan.Patron = new Patron
            {
                Id = loan.Patron.Id,
                FirstName = loan.Patron.FirstName,
                LastName = loan.Patron.LastName,
                Email = loan.Patron.Email,
                PhoneNumber = loan.Patron.PhoneNumber,
                MembershipDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.Patron.MembershipDate, DateTimeKind.Utc)),
                IsActive = loan.Patron.IsActive
            };
        }

        return grpcLoan;
    }
}
