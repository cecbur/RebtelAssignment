using BusinessModels;

namespace DataStorageContracts;

public interface ILoanRepository
{
    Task<IEnumerable<Loan>> GetAllLoans();
    Task<Loan> GetLoanById(int loanId);
    Task<IEnumerable<Loan>> GetLoansByPatronId(int patronId);
    Task<IEnumerable<Loan>> GetLoansByBookId(int bookId);
    Task<IEnumerable<Loan>> GetActiveLoans();
    Task<Loan> AddLoan(Loan loan);
    Task<Loan> UpdateLoan(Loan loan);
    Task<bool> DeleteLoan(int loanId);
}
