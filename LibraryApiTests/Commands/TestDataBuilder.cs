using BusinessModels;

namespace LibraryApiTests.Commands;

/// <summary>
/// Factory for creating test data objects with sensible defaults.
/// Each test should create its own instance to ensure test isolation.
/// </summary>
internal class TestDataBuilder
{
    private int _patronIdCounter = 1;

    public Author CreateAuthor(int id, string givenName, string surname) =>
        new() { Id = id, GivenName = givenName, Surname = surname };

    public Book CreateBook(int id, string title, Author author) =>
        new() { Id = id, Title = title, Author = author };

    public Patron CreatePatron(int id, string firstName, string lastName) =>
        new() { Id = id, FirstName = firstName, LastName = lastName };

    public Patron CreateUniquePatron()
    {
        var id = _patronIdCounter++;
        return new Patron { Id = id, FirstName = $"FirstName{id}", LastName = $"LastName{id}" };
    }

    public Loan CreateLoan(int id, Book book, Patron patron) =>
        new() { Id = id, Book = book, Patron = patron };

    public Loan[] CreateLoansForBook(Book book, int startId, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateLoan(startId + i, book, CreateUniquePatron()))
            .ToArray();
    }

    public Loan[] CreateLoansForPatron(Patron patron, int startId, int count, Author? author = null)
    {
        var bookAuthor = author ?? CreateAuthor(1, "Test", "Author");
        return Enumerable.Range(0, count)
            .Select(i =>
            {
                var book = CreateBook(startId + i, $"Book{startId + i}", bookAuthor);
                return CreateLoan(startId + i, book, patron);
            })
            .ToArray();
    }
}