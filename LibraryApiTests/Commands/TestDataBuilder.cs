using BusinessModels;

namespace LibraryApiTests.Commands;

/// <summary>
/// Factory for creating test data objects with sensible defaults.
/// Reduces duplication and makes tests more readable.
/// </summary>
internal static class TestDataBuilder
{
    private static int _patronIdCounter = 1;

    public static Author CreateAuthor(int id, string givenName, string surname) =>
        new() { Id = id, GivenName = givenName, Surname = surname };

    public static Book CreateBook(int id, string title, Author author) =>
        new() { Id = id, Title = title, Author = author };

    public static Patron CreatePatron(int id, string firstName, string lastName) =>
        new() { Id = id, FirstName = firstName, LastName = lastName };

    public static Patron CreateUniquePatron()
    {
        var id = _patronIdCounter++;
        return new Patron { Id = id, FirstName = $"FirstName{id}", LastName = $"LastName{id}" };
    }

    public static Loan CreateLoan(int id, Book book, Patron patron) =>
        new() { Id = id, Book = book, Patron = patron };

    public static Loan[] CreateLoansForBook(Book book, int startId, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateLoan(startId + i, book, CreateUniquePatron()))
            .ToArray();
    }
}