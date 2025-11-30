# Library API Architecture

This document explains the layered architecture of the Library API, focusing on how the Assignment endpoints are implemented from the API layer down to the database layer.

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Layer 1: Controllers](#layer-1-controllers)
3. [Layer 2: Commands](#layer-2-commands)
4. [Layer 3: Business Logic Grpc Client](#layer-3-business-logic-grpc-client)
5. [Layer 4: Business Logic Facade](#layer-3-business-logic-facade)
6. [Layer 5: Business Logic Services](#layer-4-business-logic-services)
7. [Layer 6: Data Storage Repositories](#layer-5-data-storage-repositories)
8. [Complete Example Flow](#complete-example-flow)
9. [Why This Architecture?](#why-this-architecture)

---

## Architecture Overview

The application follows a **layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────┐
│  Controllers (LibraryApi)           │  ← HTTP API Layer
├─────────────────────────────────────┤
│  Commands (LibraryApi)              │  ← Request Orchestration
├─────────────────────────────────────┤
│  Facade (BusinessLogic)             │  ← Unified Business Interface
├─────────────────────────────────────┤
│  Business Logic Services            │  ← Domain Logic
│  (BookPatterns, BorrowingPatterns)  │
├─────────────────────────────────────┤
│  Repositories (DataStorage)         │  ← Data Access
└─────────────────────────────────────┘
```

Each layer has a specific responsibility and communicates only with adjacent layers through well-defined interfaces.
There is no front end since this system is accessed through a REST API

---

## Layer 1: Controllers

**Location:** `LibraryApi/Controllers/AssignmentController.cs`

### What It Does
Controllers handle HTTP requests and responses. They define the API contract (routes, parameters, response codes) and delegate business logic to Commands.

### Implementation Example
```csharp
[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly GetOtherBooksBorrowedCommand _otherBooksBorrowedCommand;

    [HttpGet("other-books-borrowed/{bookId}")]
    public async Task<ActionResult<BookFrequencyResponse[]>> GetOtherBooksBorrowed(int bookId)
    {
        var (success, response) = await _otherBooksBorrowedCommand.GetOtherBooksBorrowed(bookId);
        if (success)
            return Ok(response);
        return StatusCode(500, "An error occurred");
    }
}
```

### Why This Layer Exists
- **HTTP Concerns**: Handles routing, HTTP verbs, status codes
- **API Documentation**: Swagger/OpenAPI documentation via XML comments and attributes
- **Thin Layer**: Controllers should be "thin" - they don't contain business logic
- **Separation**: Keeps HTTP concerns separate from domain logic

### Key Patterns
- Uses dependency injection to receive Command instances
- Returns appropriate HTTP status codes (200, 500)
- Delegates all logic to Commands

---

## Layer 2: Commands

**Location:** `LibraryApi/Commands/AssignmentCommands/`

### What It Does
Commands orchestrate a single API operation. They handle:
- Input validation
- Logging
- Error handling and exception translation
- Calling business logic through the gRPC Facade
- Converting between DTOs and domain models

### Implementation Example
```csharp
public class GetOtherBooksBorrowedCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<GetOtherBooksBorrowedCommand> logger)
{
    public async Task<(bool success, BookFrequencyResponse[] response)> GetOtherBooksBorrowed(int bookId)
    {
        // 1. Validation
        if (!IsValid(bookId))
            return (false, []);

        try
        {
            // 2. Logging
            _logger.LogInformation("Getting other books borrowed for book id {BookId}", bookId);

            // 3. Business Logic Call
            var bookFrequencies = await _businessLogicFacade.GetOtherBooksBorrowed(bookId);

            // 4. DTO Conversion
            var response = BookFrequencyResponseConverter.ToDto(bookFrequencies);

            _logger.LogInformation("Retrieved {Count} associated books", response.Length);
            return (true, response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting other books borrowed");
            return (false, []);
        }
    }

    private bool IsValid(int bookId) => bookId > 0;
}
```

### Why This Layer Exists
- **Single Responsibility**: Each command handles one API operation
- **Cross-Cutting Concerns**: Centralized logging, validation, error handling
- **Error Translation**: Converts infrastructure exceptions (RpcException, SqlException) to HTTP responses
- **DTO Conversion**: Translates between API DTOs and business models
- **Testability**: Easy to unit test without HTTP infrastructure

### Key Patterns
- Returns tuple `(bool success, TResponse response)` for error handling
- Catches specific exceptions (RpcException, InvalidOperationException)
- Logs at Information level for success, Error level for failures
- Validates input before calling business logic



---

## Layer 3: Business Logic Grpc Client

**Location:** `BusinessLogicGrpcClient/BusinessLogicGrpcFacade.cs`

### What It Does

This client provides transparent and seemless access to the service layer.


### Implementation Example
```csharp
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

```

### Why This Layer Exists
- **Reverse responsibility**: The Business Logic client is the only thing the API layer need in order to access the service layer 
- **Simplified access to service layer**: Commands don't need to know about gRPC. They just use the clients methods to automatically access the business logic in the service layer
- **Dependency Management**: Single dependency instead of many
- **Abstraction**: API layer doesn't know about internal business logic organization
- **Microservice architecture**: A microservice that provides a client like this is completely encapsulated in all respects
- **Intellisense**: since the client use the same interface as the facade in the service layer, the intellisense works
- **Bugs**: Errors are found at compile time instead of runtime which means they never even become bugs 


### Key Patterns
- Encapsulation - The only thing that is visible outside the microservice is its interface/facade
- Implements `IBusinessLogicFacade` interface. This is the same interface the the business logic micro service on the other side of the gRPC uses 
- Just encapsulated communication - no business logic here
- Provides dependency injection for the entire Business Logic

---

### Layer 7: Business Logic Contracts

**Location:** `BusinessLogicContracts`

### What It Does
These are the interfaces and DTO objects that API layer might need to be aware of when they use business logic in the service layer

### Implementation Example: Interface

```csharp
namespace BusinessLogicContracts.Interfaces;

public interface IBusinessLogicFacade
{
    /// <summary>
    /// 1. Inventory Insights: What are the most borrowed books? 
    /// Gets all books sorted by how many times they were loaned (most loaned first)
    /// </summary>
    /// <param name="maxBooks">Optional maximum number of books to return</param>
    /// <returns>List of books with their loan counts, ordered by loan count descending</returns>
    Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null);

    /// <summary>
    /// 2. User Activity: Which users have borrowed the most books within a given time frame?
    /// Gets patrons ordered by loan frequency within a given time frame
    /// </summary>
    /// <param name="startDate">Start date of the time frame</param>
    /// <param name="endDate">End date of the time frame</param>
    /// <param name="maxPatrons">The maximum number of patrons to return</param>
    /// <returns>List of patrons ordered by loan count (descending)</returns>
    /// <response code="200">List of patrons ordered by loan count (descending)</response>
    /// <response code="500">If there was an internal server error</response>
    Task<PatronLoans[]> GetPatronsOrderedByLoanFrequency(DateTime startDate, DateTime endDate);


```

### Implementation Example: DTO

```csharp
namespace BusinessLogicContracts.Dto;

    public class BookFrequency
    {
        public required Book AssociatedBook { get; set; }
        public double LoansOfThisBookPerLoansOfMainBook { get; set; }
    }

```

### Why This Layer Exists
- **Domain Logic**: Encapsulates access to service layer
- **Maintainability**: Publicly available interfaces in one place, easy to understand and hard to modify by mistake
- **Technology Independence**: Only interfaces and DTOs are available publicly
- **Testability**: Makes it easy to mock the service layer
- **Reusability**: Business logic endpoints can be reused by multiple commands or applications

### Key Patterns
- **Separation of Concerns**: No other class use anything in BusinessLogic. BusinessLogic displays exactly this facade and no other


---

## Layer 4: Business Logic Facade

**Location:** `BusinessLogic/Facade.cs`

### What It Does
The Facade provides a **unified interface** to all business logic services. It's a single entry point for the API layer to access business operations.

### Implementation Example
```csharp
public class Facade(
    BookPatterns bookPatterns,
    PatronActivity patronActivity,
    BorrowingPatterns borrowingPatterns) : IBusinessLogicFacade
{
    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null) =>
        await bookPatterns.GetBooksSortedByMostLoaned(maxBooksToReturn);

    public async Task<PatronLoans[]> GetPatronsOrderedByLoanFrequency(DateTime startDate, DateTime endDate) =>
        await patronActivity.GetPatronsOrderedByLoanFrequency(startDate, endDate);

    public async Task<BookFrequency[]> GetOtherBooksBorrowed(int bookId) =>
        await borrowingPatterns.GetOtherBooksBorrowed(bookId);
}
```

### Why This Layer Exists
- **Simplified API**: Commands don't need to know about multiple business service classes
- **Dependency Management**: Single dependency instead of many
- **Abstraction**: API layer doesn't know about internal business logic organization
- **Interface Segregation**: Can expose different facades for different clients
- **Future Evolution**: Easy to reorganize business logic without affecting Commands

### Key Patterns
- Implements `IBusinessLogicFacade` interface
- Simple delegation - no business logic here
- Uses primary constructor (C# 12) for dependency injection

---

## Layer 5: Business Logic Services

**Location:** `BusinessLogic/BorrowingPatterns.cs`, `BusinessLogic/BookPatterns.cs`

### What It Does
Business Logic Services contain the **domain logic**. They:
- Implement business rules
- Orchestrate data from multiple repositories
- Perform calculations and transformations
- Apply filtering, sorting, and business-specific logic

### Implementation Example: BorrowingPatterns

```csharp
public class BorrowingPatterns(
    ILoanRepository loanRepository,
    IBorrowingPatternRepository borrowingPatternRepository)
{
    public async Task<BookFrequency[]> GetOtherBooksBorrowed(int bookId)
    {
        // 1. Get data from repositories
        var mainBookLoans = await _loanRepository.GetLoansByBookId(bookId);
        var mainBookLoanCount = mainBookLoans.Count();
        var associatedBooksData = await _borrowingPatternRepository.GetOtherBooksBorrowed(bookId);

        // 2. Apply business rules
        var significantBooks = FilterSignificantBooks(associatedBooksData);
        var sortedBooks = SortByFrequency(significantBooks);
        var bookFrequencies = MapToBookFrequencies(sortedBooks, mainBookLoanCount);

        return bookFrequencies;
    }

    private static AssociatedBooks.BookCount[] FilterSignificantBooks(
        AssociatedBooks associatedBooksData)
    {
        // Business Rule: Only show books borrowed more than once
        return associatedBooksData.Associated
            .Where(bookData => bookData.Count > 1)
            .ToArray();
    }

    private static BookFrequency[] MapToBookFrequencies(
        AssociatedBooks.BookCount[] sortedBooks,
        int mainBookLoanCount)
    {
        // Business Logic: Calculate loan ratio
        return sortedBooks
            .Select(bookData => new BookFrequency
            {
                AssociatedBook = bookData.Book,
                LoansOfThisBookPerLoansOfMainBook = (double)bookData.Count / mainBookLoanCount
            })
            .ToArray();
    }
}
```

### Implementation Example: BookPatterns

```csharp
public class BookPatterns(ILoanRepository loanRepository)
{
    public async Task<BookLoans[]> GetBooksSortedByMostLoaned(int? maxBooksToReturn = null)
    {
        // 1. Get all loans
        var allLoans = await _loanRepository.GetAllLoans();

        // 2. Group by book
        var bookLoanGroups = allLoans.GroupBy(loan => loan.Book);

        // 3. Sort by count (descending)
        var sortedBookLoanGroups = SortByLoanCountDescending(bookLoanGroups);

        // 4. Apply limit (if specified)
        var limitedBookLoanGroups = ApplyResultLimit(sortedBookLoanGroups, maxBooksToReturn);

        // 5. Map to DTO
        var bookLoansResults = MapToBookLoansDto(limitedBookLoanGroups);

        return bookLoansResults;
    }

    private static IGrouping<Book, Loan>[] SortByLoanCountDescending(
        IEnumerable<IGrouping<Book, Loan>> bookLoanGroups)
    {
        return bookLoanGroups
            .OrderByDescending(group => group.Count())
            .ToArray();
    }
}
```

### Why This Layer Exists
- **Domain Logic**: Encapsulates business rules and calculations
- **Testability**: Pure business logic without infrastructure concerns
- **Reusability**: Business logic can be reused by multiple commands or applications
- **Maintainability**: Business rules in one place, easy to understand and modify
- **Technology Independence**: No knowledge of HTTP, databases, or other infrastructure

### Key Patterns
- **Small, focused methods**: Each method does one thing
- **Descriptive names**: Methods explain what they do (`FilterSignificantBooks`, `SortByFrequency`)
- **Repository composition**: Can call multiple repositories to get needed data
- **LINQ for transformations**: Uses LINQ for filtering, sorting, mapping

---

## Layer 6: Data Storage Grpc Client

**Location:** `DataStorageGrpcClient`

### What It Does

This client provides transparent and seemless access to the data storage layer.


### Implementation Example
```csharp
namespace DataStorageGrpcClient;

public class BorrowingPatternRepository : DataStorageContracts.IBorrowingPatternRepository
{
    private readonly GrpcChannel _channel;
    private readonly DataStorage.Grpc.BorrowingPatternService.BorrowingPatternServiceClient _client;

    public BorrowingPatternRepository(string serverAddress)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = new DataStorage.Grpc.BorrowingPatternService.BorrowingPatternServiceClient(_channel);
    }

    public async Task<DataStorageContracts.Dto.AssociatedBooks> GetOtherBooksBorrowed(int bookId)
    {
        try
        {
            var request = new DataStorage.Grpc.GetOtherBooksBorrowedRequest { BookId = bookId };
            var response = await _client.GetOtherBooksBorrowedAsync(request);
            return MapFromGrpcAssociatedBooks(response.AssociatedBooks);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException($"Error getting other books borrowed for book id {bookId}", ex);
        }
    }



```

### Why This Layer Exists
- **Reverse responsibility**: The client is the only thing other parts of the solution need in order to access data storage 
- **Simplified access to service layer**: Other classes don't need to know about gRPC. They just use the clients methods to automatically access the data storage layer
- **Dependency Management**: Single dependency instead of many
- **Abstraction**: No other layers know about internal data storag organization
- **Microservice architecture**: A microservice that provides a client like this is completely encapsulated in all respects
- **Intellisense**: since the client use the same interface as the repository in the data storage, the intellisense works
- **Bugs**: Errors are found at compile time instead of runtime which means they never even become bugs 


### Key Patterns
- Encapsulation - The only thing that is visible outside the DataStorage microservice is its interface/facade
- Implements the interfaces `IBookRepository`, `ILoanRepository` and `IBorrowingPatternRepository`. These are the same interfaces that the DataStorage micro service uses on the other side of the gRPC 
- Just encapsulated communication - no business logic here
- Provides dependency injection for the entire DataStorage

---

### Layer 7: Data Storage Contracts

**Location:** `DataStorageContracts`

### What It Does
These are the interfaces and DTO objects that other classes might need to be aware of when they use DataStorageClient

### Implementation Example: Interface

```csharp
namespace DataStorageContracts;

public interface ILoanRepository
{
    Task<IEnumerable<Loan>> GetAllLoans();
    Task<Loan> GetLoanById(int loanId);
    Task<IEnumerable<Loan>> GetLoansByPatronId(int patronId);
    Task<IEnumerable<Loan>> GetLoansByBookId(int bookId);
    Task<IEnumerable<Loan>> GetActiveLoans();
    Task<IEnumerable<Loan>> GetLoansByTime(DateTime startDate, DateTime endDate);
    Task<Loan> AddLoan(Loan loan);
    Task<Loan> UpdateLoan(Loan loan);
    Task<bool> DeleteLoan(int loanId);
}



```

### Implementation Example: DTO

```csharp
public class AssociatedBooks
{
    public Book Book { get; set; }

    public BookCount[] Associated { get; set; }
    
    public class BookCount
    {
        public Book Book { get; set; }
        public int Count { get; set; }
    }
}

```

### Why This Layer Exists
- **Domain Logic**: Encapsulates database access
- **Testability**: Makes it easy to mock data storage
- **Reusability**: Data storage endpoints can be reused by multiple commands or applications
- **Maintainability**: Publicly available interfaces in one place, easy to understand and hard to modify by mistake
- **Technology Independence**: Only interfaces and DTOs are available publicly

### Key Patterns
- **Separation of Concerns**: No other class use anything in DataStorage. DataStorage displays exactly this facade and no other
- **Descriptive names**: Methods explain what they do (`GetAllBooks`, `GetBookById`)



---

# Layer 6: Data Storage Repositories

**Location:** `DataStorage/Repositories/`, `DataStorage/RepositoriesMultipleTables/`

### What It Does
Repositories handle **data access**. They:
- Execute SQL queries
- Map database entities to domain models
- Handle database connections
- Provide data to business logic layer

### Implementation Example: LoanRepository

```csharp
public class LoanRepository(IDbConnectionFactory connectionFactory) : BaseRepository(connectionFactory), ILoanRepository
{
    public async Task<IEnumerable<BusinessModels.Loan>> GetAllLoans()
    {
        const string sql = @"
            SELECT
                l.Id, l.BookId, l.PatronId, l.LoanDate, l.DueDate, l.ReturnDate, l.IsReturned,
                b.Id, b.Title, b.AuthorId, b.ISBN, b.PublicationYear, b.NumberOfPages, b.IsAvailable,
                a.Id, a.GivenName, a.Surname,
                p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.MembershipDate, p.IsActive
            FROM Loan l
            LEFT JOIN Book b ON l.BookId = b.Id
            LEFT JOIN Author a ON b.AuthorId = a.Id
            LEFT JOIN Patron p ON l.PatronId = p.Id
            ORDER BY l.LoanDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var loanDictionary = new Dictionary<int, (Entities.Loan, Entities.Book, Entities.Author?, Entities.Patron)>();

        await connection.QueryAsync<Entities.Loan, Entities.Book, Entities.Author, Entities.Patron, int>(
            sql,
            (loan, book, author, patron) =>
            {
                loanDictionary[loan.Id] = (loan, book, author, patron);
                return 0;
            },
            splitOn: "Id,Id,Id");

        return loanDictionary.Values
            .Select(tuple => LoanConverter.ToModel(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
    }
}
```

### Implementation Example: BorrowingPatternRepository

This repository handles **complex queries spanning multiple tables**:

```csharp
public class BorrowingPatternRepository(
    IDbConnectionFactory connectionFactory,
    IBookRepository bookRepository) : BaseRepository(connectionFactory)
{
    public async Task<AssociatedBooks> GetOtherBooksBorrowed(int bookId)
    {
        // 1. Complex SQL query to find association patterns
        const string associatedSql = @"
            SELECT
                otherBook.Id as BookId,
                COUNT(otherBook.Id) as BorrowCount
            FROM Loan l
            INNER JOIN Patron p ON l.PatronId = p.Id
            INNER JOIN Loan otherLoan ON otherLoan.PatronId = p.Id
            INNER JOIN Book otherBook ON otherLoan.BookId = otherBook.Id
            WHERE l.BookId = @BookId
                AND otherBook.Id <> @BookId
            GROUP BY otherBook.Id
            ORDER BY BorrowCount DESC";

        using var connection = _connectionFactory.CreateConnection();

        // 2. Get book IDs and counts
        var associatedBookData = await connection.QueryAsync<(int BookId, int BorrowCount)>(
            associatedSql,
            new { BookId = bookId });

        // 3. Fetch full book objects from BookRepository
        var associatedBookIds = associatedBookData.Select(x => x.BookId).ToList();
        var books = await _bookRepository.GetBooksByIds(associatedBookIds.Concat([bookId]));
        var bookArray = books.ToArray();
        var mainBook = bookArray.First(b => b.Id == bookId);
        var bookDictionary = bookArray.Except([mainBook]).ToDictionary(b => b.Id);

        // 4. Build result with counts
        var associatedBooksList = new List<AssociatedBooks.BookCount>();
        foreach (var (bookIdValue, count) in associatedBookData)
        {
            if (bookDictionary.TryGetValue(bookIdValue, out var book))
            {
                associatedBooksList.Add(new AssociatedBooks.BookCount
                {
                    Book = book,
                    Count = count
                });
            }
        }

        return new AssociatedBooks
        {
            Book = mainBook,
            Associated = associatedBooksList.ToArray()
        };
    }
}
```

### Why This Layer Exists
- **Data Access Abstraction**: Business logic doesn't know about SQL or Dapper
- **Query Optimization**: SQL queries optimized for specific use cases
- **Entity Mapping**: Converts database entities to rich domain models
- **Connection Management**: Handles database connections properly
- **Testability**: Can be mocked for business logic testing

### Key Patterns
- **Dapper for ORM**: Lightweight, performant SQL mapping
- **Connection Factory Pattern**: `IDbConnectionFactory` for connection creation
- **Multi-mapping**: Dapper's multi-mapping for JOIN queries
- **Dictionary for deduplication**: Prevents duplicate objects in memory
- **Converter classes**: Separate classes handle entity-to-model conversion
- **Repository Composition**: Repositories can call other repositories (e.g., `BorrowingPatternRepository` calls `BookRepository`)

---

## Complete Example Flow

Let's trace a request through all layers for: **GET /api/assignment/other-books-borrowed/123**

### Step 1: Controller Receives Request
```
HTTP GET /api/assignment/other-books-borrowed/123
↓
AssignmentController.GetOtherBooksBorrowed(bookId: 123)
```

### Step 2: Controller Delegates to Command
```csharp
var (success, response) = await _otherBooksBorrowedCommand.GetOtherBooksBorrowed(123);
```

### Step 3: Command Validates and Calls Facade
```csharp
GetOtherBooksBorrowedCommand:
  1. Validates: bookId > 0 ✓
  2. Logs: "Getting other books borrowed for book id 123"
  3. Calls: await _businessLogicFacade.GetOtherBooksBorrowed(123)
```

### Step 4: Facade Delegates to Business Service
```csharp
Facade.GetOtherBooksBorrowed(123)
  → borrowingPatterns.GetOtherBooksBorrowed(123)
```

### Step 5: Business Service Orchestrates Logic
```csharp
BorrowingPatterns.GetOtherBooksBorrowed(123):
  1. Get main book loan count
     ↓ _loanRepository.GetLoansByBookId(123)

  2. Get associated books with counts
     ↓ _borrowingPatternRepository.GetOtherBooksBorrowed(123)

  3. Filter significant books (count > 1)

  4. Sort by frequency (descending)

  5. Calculate loan ratios

  6. Return BookFrequency[]
```

### Step 6: Repositories Execute SQL
```csharp
LoanRepository.GetLoansByBookId(123):
  SQL: SELECT l.*, b.*, a.*, p.*
       FROM Loan l
       JOIN Book b ON l.BookId = b.Id
       ...
       WHERE l.BookId = @BookId
  ↓
  Returns: IEnumerable<Loan>

BorrowingPatternRepository.GetOtherBooksBorrowed(123):
  SQL: SELECT otherBook.Id, COUNT(*)
       FROM Loan l
       JOIN Patron p ON l.PatronId = p.Id
       JOIN Loan otherLoan ON otherLoan.PatronId = p.Id
       JOIN Book otherBook ON otherLoan.BookId = otherBook.Id
       WHERE l.BookId = @BookId AND otherBook.Id <> @BookId
       GROUP BY otherBook.Id
  ↓
  Returns: AssociatedBooks
```

### Step 7: Response Flows Back Up
```
BorrowingPatternRepository → BookFrequency[]
  ↓
BorrowingPatterns → BookFrequency[]
  ↓
Facade → BookFrequency[]
  ↓
GetOtherBooksBorrowedCommand → (true, BookFrequencyResponse[])
  ↓
AssignmentController → HTTP 200 OK + JSON
```

### Data Flow Visualization
```
Request:  bookId=123
  ↓
[Controller] → validate route/params
  ↓
[Command] → validate business rules, log
  ↓
[Facade] → route to correct service
  ↓
[Business Logic] → apply domain rules:
  • Filter books with count > 1
  • Sort by frequency
  • Calculate ratios
  ↓
[Repository] → execute SQL, map entities
  ↓
Database → return rows
  ↓
Response: [
  { book: "Animal Farm", ratio: 0.75 },
  { book: "Brave New World", ratio: 0.5 }
]
```

---

## Why This Architecture?

### 1. Separation of Concerns
Each layer has a single, well-defined responsibility:
- Controllers → HTTP
- Commands → Orchestration
- Business Logic → Domain rules
- Repositories → Data access

### 2. Testability
- **Unit Tests**: Business logic can be tested without databases or HTTP
- **Integration Tests**: Repositories tested against real databases
- **API Tests**: Controllers tested without real business logic

Example test structure:
```
LibraryApiTests/        ← Test Commands with mocked Facade
BusinessLogicTests/     ← Test services with mocked Repositories
DataStorageIntegrationTests/ ← Test Repositories with real DB
```

### 3. Maintainability
- **Clear Dependencies**: Each layer depends only on interfaces from the layer below
- **Easy to Find Code**: Predictable structure
- **Easy to Change**: Change one layer without affecting others

### 4. Scalability
- **Horizontal Scaling**: Stateless design allows multiple API instances
- **Caching**: Can add caching at Facade or Repository level
- **Future Evolution**: Can add gRPC, message queues, etc. without changing business logic

### 5. Domain-Driven Design
- **Business Logic is Central**: Not tied to framework or infrastructure
- **Rich Domain Models**: Business objects (Book, Loan, Patron) are meaningful
- **Ubiquitous Language**: Code uses business terms

### 6. Dependency Inversion Principle
```
High-level modules (Business Logic) don't depend on low-level modules (Repositories)
Both depend on abstractions (Interfaces)
```

Example:
```csharp
// Business logic depends on interface, not concrete implementation
public class BorrowingPatterns(ILoanRepository loanRepository) { }

// Repository implements interface
public class LoanRepository : ILoanRepository { }
```

### 7. Single Responsibility Principle
Each class has one reason to change:
- `AssignmentController` changes if API contract changes
- `GetOtherBooksBorrowedCommand` changes if orchestration logic changes
- `BorrowingPatterns` changes if business rules change
- `BorrowingPatternRepository` changes if query needs optimization

### Trade-offs
This architecture has more layers than a simple approach, which means:
- **More files**: Can be overwhelming initially
- **More indirection**: Need to trace through multiple layers
- **Boilerplate code**: Facade is mostly delegation

However, for medium-to-large applications, the benefits outweigh the costs:
- **Lower bug risk**: Clear separation prevents mixing concerns
- **Easier onboarding**: Predictable structure
- **Better long-term maintainability**: Changes are localized

---

## Summary

The Library API uses a **5-layer architecture**:

1. **Controllers**: HTTP API endpoints
2. **Commands**: Request orchestration, validation, error handling
3. **Facade**: Unified business logic interface
4. **Business Services**: Domain logic and rules
5. **Repositories**: Data access and SQL

Each layer has a clear purpose, communicates through interfaces, and can be tested independently. This design prioritizes **maintainability**, **testability**, and **separation of concerns** over simplicity.
