using BusinessModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DataStorageIntegrationTests;

/// <summary>
/// Helper class for generating test data in integration tests.
/// Reduces boilerplate for creating Authors, Books, Patrons, and Loans.
/// Returns full objects from database using OUTPUT INSERTED to ensure tests assert against actual DB state.
/// </summary>
public class TestDataGenerator(string connectionString)
{
    /// <summary>
    /// Creates an author and returns the object inserted in the database.
    /// </summary>
    public async Task<Author> CreateAuthor(string? givenName, string surname)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<Author>(@"
            INSERT INTO Author (GivenName, Surname)
            OUTPUT INSERTED.*
            VALUES (@GivenName, @Surname);",
            new { GivenName = givenName, Surname = surname });
    }

    /// <summary>
    /// Creates a book and returns the object inserted in the database (without Author navigation property).
    /// </summary>
    public async Task<Book> CreateBook(
        string title,
        int authorId,
        string? isbn = null,
        int? publicationYear = null,
        int? numberOfPages = null)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // First, insert and get the ID
        var book = await connection.QuerySingleAsync<Book>(@"
            INSERT INTO Book (Title, AuthorId, ISBN, PublicationYear, NumberOfPages)
            OUTPUT INSERTED.*
            VALUES (@Title, @AuthorId, @ISBN, @PublicationYear, @NumberOfPages);",
            new
            {
                Title = title,
                AuthorId = authorId,
                ISBN = isbn,
                PublicationYear = publicationYear,
                NumberOfPages = numberOfPages
            });

        // Note: Author navigation property is not populated (would require separate query)
        return book;
    }

    /// <summary>
    /// Creates a patron and returns the full object as inserted in the database.
    /// </summary>
    public async Task<Patron> CreatePatron(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<Patron>(@"
            INSERT INTO Patron (FirstName, LastName, Email, PhoneNumber)
            OUTPUT INSERTED.*
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber);",
            new { FirstName = firstName, LastName = lastName, Email = email, PhoneNumber = phoneNumber });
    }

    /// <summary>
    /// Creates a loan and returns the ID (full Loan object would require joins for Book/Patron navigation properties).
    /// </summary>
    public async Task<Loan> CreateLoan(
        int bookId,
        int patronId,
        DateTime loanDate,
        DateTime dueDate,
        DateTime? returnDate = null,
        bool isReturned = true)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<Loan>(@"
            INSERT INTO Loan (BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned)
            OUTPUT INSERTED.*
            VALUES (@BookId, @PatronId, @LoanDate, @DueDate, @ReturnDate, @IsReturned);",
            new
            {
                BookId = bookId,
                PatronId = patronId,
                LoanDate = loanDate,
                DueDate = dueDate,
                ReturnDate = returnDate,
                IsReturned = isReturned
            });
    }

    /// <summary>
    /// Creates multiple loans in a single database call for better performance.
    /// </summary>
    public async Task CreateLoans(IEnumerable<LoanData> loans)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO Loan (BookId, PatronId, LoanDate, DueDate, ReturnDate, IsReturned)
            VALUES (@BookId, @PatronId, @LoanDate, @DueDate, @ReturnDate, @IsReturned)",
            loans);
    }

    public record LoanData(
        int BookId,
        int PatronId,
        DateTime LoanDate,
        DateTime DueDate,
        DateTime? ReturnDate,
        bool IsReturned);
}
