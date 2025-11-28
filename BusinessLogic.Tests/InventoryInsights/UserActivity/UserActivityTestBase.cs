using BusinessModels;
using DataStorageContracts;
using Moq;

namespace BusinessLogic.Tests.InventoryInsights.UserActivity;

public abstract class UserActivityTestBase
{
    protected readonly Mock<ILoanRepository> MockLoanRepository;
    protected readonly BusinessLogic.InventoryInsights.UserActivity UserActivity;

    protected UserActivityTestBase()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        UserActivity = new BusinessLogic.InventoryInsights.UserActivity(MockLoanRepository.Object);
    }

    protected static Patron CreatePatron(int id, string firstName = "Test", string lastName = "User")
    {
        return new Patron
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
            MembershipDate = new DateTime(2023, 1, 1),
            IsActive = true
        };
    }

    protected static Book CreateBook(int id, string title, int? numberOfPages = null)
    {
        return new Book
        {
            Id = id,
            Title = title,
            NumberOfPages = numberOfPages,
            Isbn = $"978-{id:D10}",
            PublicationYear = 2020
        };
    }

    protected static Loan CreateLoan(
        int id,
        Book book,
        Patron patron,
        DateTime loanDate,
        DateTime dueDate,
        DateTime? returnDate = null,
        bool isReturned = false)
    {
        return new Loan
        {
            Id = id,
            Book = book,
            Patron = patron,
            LoanDate = loanDate,
            DueDate = dueDate,
            ReturnDate = returnDate,
            IsReturned = isReturned
        };
    }
}
