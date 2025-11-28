using BusinessModels;
using DataStorage.Grpc;
using DataStorageContracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace DataStorageClient;

public class LoanRepository : ILoanRepository
{
    private readonly GrpcChannel _channel;
    private readonly LoanService.LoanServiceClient _client;

    public LoanRepository(string serverAddress)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = new LoanService.LoanServiceClient(_channel);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetAllLoans()
    {
        var request = new GetAllLoansRequest();
        var response = await _client.GetAllLoansAsync(request);
        return response.Loans.Select(MapFromGrpcLoan);
    }

    public async Task<BusinessModels.Loan> GetLoanById(int loanId)
    {
        try
        {
            var request = new GetLoanByIdRequest { LoanId = loanId };
            var response = await _client.GetLoanByIdAsync(request);
            return MapFromGrpcLoan(response.Loan);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Loan with id {loanId} not found", ex);
        }
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByPatronId(int patronId)
    {
        var request = new GetLoansByPatronIdRequest { PatronId = patronId };
        var response = await _client.GetLoansByPatronIdAsync(request);
        return response.Loans.Select(MapFromGrpcLoan);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetLoansByBookId(int bookId)
    {
        var request = new GetLoansByBookIdRequest { BookId = bookId };
        var response = await _client.GetLoansByBookIdAsync(request);
        return response.Loans.Select(MapFromGrpcLoan);
    }

    public async Task<IEnumerable<BusinessModels.Loan>> GetActiveLoans()
    {
        var request = new GetActiveLoansRequest();
        var response = await _client.GetActiveLoansAsync(request);
        return response.Loans.Select(MapFromGrpcLoan);
    }

    public async Task<BusinessModels.Loan> AddLoan(BusinessModels.Loan loan)
    {
        var request = new AddLoanRequest
        {
            BookId = loan.Book?.Id ?? 0,
            PatronId = loan.Patron?.Id ?? 0,
            LoanDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.LoanDate, DateTimeKind.Utc)),
            DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.DueDate, DateTimeKind.Utc)),
            IsReturned = loan.IsReturned
        };

        if (loan.ReturnDate.HasValue)
        {
            request.ReturnDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.ReturnDate.Value, DateTimeKind.Utc));
        }

        var response = await _client.AddLoanAsync(request);
        return MapFromGrpcLoan(response.Loan);
    }

    public async Task<BusinessModels.Loan> UpdateLoan(BusinessModels.Loan loan)
    {
        try
        {
            var request = new UpdateLoanRequest
            {
                Id = loan.Id,
                BookId = loan.Book?.Id ?? 0,
                PatronId = loan.Patron?.Id ?? 0,
                LoanDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.LoanDate, DateTimeKind.Utc)),
                DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.DueDate, DateTimeKind.Utc)),
                IsReturned = loan.IsReturned
            };

            if (loan.ReturnDate.HasValue)
            {
                request.ReturnDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loan.ReturnDate.Value, DateTimeKind.Utc));
            }

            var response = await _client.UpdateLoanAsync(request);
            return MapFromGrpcLoan(response.Loan);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Loan with id {loan.Id} not found", ex);
        }
    }

    public async Task<bool> DeleteLoan(int loanId)
    {
        var request = new DeleteLoanRequest { LoanId = loanId };
        var response = await _client.DeleteLoanAsync(request);
        return response.Success;
    }

    private static BusinessModels.Loan MapFromGrpcLoan(DataStorage.Grpc.Loan grpcLoan)
    {
        var book = new BusinessModels.Book
        {
            Id = grpcLoan.Book.Id,
            Title = grpcLoan.Book.Title,
            Isbn = grpcLoan.Book.Isbn,
            PublicationYear = grpcLoan.Book.PublicationYear,
            NumberOfPages = grpcLoan.Book.NumberOfPages,
            IsAvailableForLoan = grpcLoan.Book.IsAvailable
        };

        if (grpcLoan.Book.Author != null)
        {
            book.Author = new BusinessModels.Author
            {
                Id = grpcLoan.Book.Author.Id,
                GivenName = grpcLoan.Book.Author.GivenName,
                Surname = grpcLoan.Book.Author.Surname
            };
        }

        var patron = new BusinessModels.Patron
        {
            Id = grpcLoan.Patron.Id,
            FirstName = grpcLoan.Patron.FirstName,
            LastName = grpcLoan.Patron.LastName,
            Email = grpcLoan.Patron.Email,
            PhoneNumber = grpcLoan.Patron.PhoneNumber,
            MembershipDate = grpcLoan.Patron.MembershipDate.ToDateTime(),
            IsActive = grpcLoan.Patron.IsActive
        };

        var loan = new BusinessModels.Loan
        {
            Id = grpcLoan.Id,
            Book = book,
            Patron = patron,
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