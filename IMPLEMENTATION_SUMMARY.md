# Implementation Summary

## Overview
This document provides a quick reference for the Library Management System implementation.

## Four Core Business Functions

### 1. Check if Book ID is Power of Two
**Location:** `BusinessLogic/BookService.cs:18`

**Implementation:**
```csharp
public bool IsPowerOfTwo(int bookId)
{
    if (bookId <= 0)
    {
        throw new ArgumentException("Book ID must be a positive integer", nameof(bookId));
    }
    return (bookId & (bookId - 1)) == 0;
}
```

**API Endpoint:** `GET /api/BorrowingPatterns/is-power-of-two/{bookId}`

**Algorithm:** Uses bitwise AND operation. A power of two has exactly one bit set in binary representation.
- Example: 8 (binary: 1000) & 7 (binary: 0111) = 0 ✓
- Example: 6 (binary: 0110) & 5 (binary: 0101) = 4 ✗

### 2. Reverse Book Title
**Location:** `BusinessLogic/BookService.cs:35`

**Implementation:**
```csharp
public string ReverseTitle(string title)
{
    if (title == null)
    {
        throw new ArgumentNullException(nameof(title), "Title cannot be null");
    }
    char[] charArray = title.ToCharArray();
    Array.Reverse(charArray);
    return new string(charArray);
}
```

**API Endpoint:** `POST /api/Book/reverse-title`

**Example:**
- Input: "Moby Dick"
- Output: "kciD yboM"

### 3. Generate Title Replicas
**Location:** `BusinessLogic/BookService.cs:56`

**Implementation:**
```csharp
public string GenerateTitleReplicas(string title, int count)
{
    if (title == null)
    {
        throw new ArgumentNullException(nameof(title), "Title cannot be null");
    }
    if (count < 0)
    {
        throw new ArgumentException("Count must be non-negative", nameof(count));
    }
    if (count == 0)
    {
        return string.Empty;
    }
    return string.Concat(Enumerable.Repeat(title, count));
}
```

**API Endpoint:** `POST /api/Book/generate-replicas`

**Example:**
- Input: "Read", 3
- Output: "ReadReadRead"

### 4. List Odd-Numbered Book IDs
**Location:** `BusinessLogic/BookService.cs:88`

**Implementation:**
```csharp
public IEnumerable<int> GetOddNumberedBookIds(IEnumerable<int> bookIds)
{
    if (bookIds == null)
    {
        throw new ArgumentNullException(nameof(bookIds), "Book IDs collection cannot be null");
    }
    return bookIds.Where(id => id % 2 != 0);
}
```

**API Endpoint:** `GET /api/BorrowingPatterns/odd-book-ids`

**Algorithm:** Uses LINQ Where clause with modulo operator to filter odd numbers.

## Clean Code Principles Applied

### 1. Meaningful Names
- Classes: `BookService`, `BookRepository`, `LibraryDbContext`
- Methods: `IsPowerOfTwo`, `ReverseTitle`, `GenerateTitleReplicas`
- Variables: `bookId`, `titlePattern`, `connectionString`

### 2. Functions Do One Thing
Each method has a single responsibility:
- `IsPowerOfTwo` - Only checks power of two
- `ReverseTitle` - Only reverses strings
- `AddBookAsync` - Only adds books

### 3. DRY (Don't Repeat Yourself)
- Repository pattern eliminates duplicate database code
- DTOs centralize data transfer logic
- Mapping functions (`MapToDto`, `MapToEntity`) prevent code duplication

### 4. Error Handling
- All methods validate input parameters
- Proper exception throwing with descriptive messages
- Try-catch blocks in controllers with logging

### 5. Comments and Documentation
- XML documentation on all public methods
- Inline comments explain complex logic
- README provides comprehensive documentation

## SOLID Principles Implementation

### Single Responsibility Principle (SRP)
Each class has one reason to change:
- `BookService` - Business logic only
- `BookRepository` - Data access only
- `BookController` - HTTP handling only
- `LibraryDbContext` - Database configuration only

### Open/Closed Principle (OCP)
- `IBookService` interface allows new implementations without changing existing code
- `IBookRepository` interface enables different data sources
- Controllers depend on abstractions, not concrete implementations

### Liskov Substitution Principle (LSP)
- Any `IBookService` implementation can replace `BookService`
- Any `IBookRepository` implementation can replace `BookRepository`
- All implementations honor the interface contracts

### Interface Segregation Principle (ISP)
- `IBookService` - Only book business logic methods
- `IBookRepository` - Only data access methods
- Controllers only depend on methods they use

### Dependency Inversion Principle (DIP)
```csharp
// High-level module depends on abstraction
public class BookController
{
    private readonly IBookService _bookService;  // Not BookService
    private readonly IBookRepository _repository; // Not BookRepository
}
```

## Architecture Layers

```
┌─────────────────────────────────────┐
│        LibraryApi (Web API)         │
│  Controllers, DTOs, Middleware      │
└──────────────┬──────────────────────┘
               │ Dependency Injection
               ↓
┌─────────────────────────────────────┐
│    BusinessLogic (Domain Layer)     │
│   Services, Business Rules          │
└──────────────┬──────────────────────┘
               │ Dependency Injection
               ↓
┌─────────────────────────────────────┐
│   DataStorage (Data Access Layer)   │
│ Repositories, Dapper, SQL Queries   │
└──────────────┬──────────────────────┘
               │ Dapper + Explicit SQL
               ↓
┌─────────────────────────────────────┐
│    SQL Server LocalDB (Database)    │
│      Library Database, Book Table   │
└─────────────────────────────────────┘
```

## Database Schema

### Book Table
```sql
CREATE TABLE [Book] (
    [BookId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Author] NVARCHAR(100) NULL,
    [ISBN] NVARCHAR(20) NULL,
    [PublicationYear] INT NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [UQ_Book_ISBN] UNIQUE ([ISBN])
);
```

## API Endpoints Summary

| Method | Endpoint | Description | Location |
|--------|----------|-------------|----------|
| GET | /api/BorrowingPatterns/analyze | Analyze borrowing patterns | `BorrowingPatternsController.cs:33` |
| GET | /api/BorrowingPatterns/is-power-of-two/{id} | Check if ID is power of two | `BorrowingPatternsController.cs:76` |
| GET | /api/BorrowingPatterns/odd-book-ids | Get odd book IDs | `BorrowingPatternsController.cs:101` |
| GET | /api/Book | Get all books | `BookController.cs:33` |
| GET | /api/Book/{id} | Get book by ID | `BookController.cs:47` |
| GET | /api/Book/search | Search books by title | `BookController.cs:117` |
| POST | /api/Book | Create new book | `BookController.cs:131` |
| PUT | /api/Book/{id} | Update book | `BookController.cs:155` |
| DELETE | /api/Book/{id} | Delete book | `BookController.cs:185` |
| POST | /api/Book/reverse-title | Reverse a title | `BookController.cs:65` |
| POST | /api/Book/generate-replicas | Generate title replicas | `BookController.cs:89` |

## Testing the APIs

### Quick Start
1. Run `SetupDatabase.bat` to create the database
2. Run `dotnet run` in the LibraryApi folder
3. Open browser to `https://localhost:7000`
4. Use Swagger UI to test endpoints

### Example Requests

**Get All Books:**
```bash
curl https://localhost:7000/api/Book
```

**Check Power of Two:**
```bash
curl https://localhost:7000/api/BorrowingPatterns/is-power-of-two/16
# Response: true
```

**Reverse Title:**
```bash
curl -X POST https://localhost:7000/api/Book/reverse-title \
  -H "Content-Type: application/json" \
  -d '{"title":"Moby Dick"}'
# Response: "kciD yboM"
```

**Generate Replicas:**
```bash
curl -X POST https://localhost:7000/api/Book/generate-replicas \
  -H "Content-Type: application/json" \
  -d '{"title":"Read","count":3}'
# Response: "ReadReadRead"
```

**Analyze Patterns:**
```bash
curl https://localhost:7000/api/BorrowingPatterns/analyze
# Response: {
#   "powerOfTwoBookIds": [1, 2, 4, 8, 16],
#   "oddNumberedBookIds": [1, 3, 5, 7, 9, 11, 13, 15],
#   "totalBooksAnalyzed": 16
# }
```

## Key Technologies

- **.NET 8** - Latest LTS version
- **ASP.NET Core Web API** - RESTful API framework
- **Dapper 2.1** - Lightweight micro-ORM for high performance
- **Microsoft.Data.SqlClient** - SQL Server data provider
- **SQL Server LocalDB** - Lightweight database
- **Swagger/OpenAPI** - API documentation
- **Dependency Injection** - Built-in IoC container
- **Explicit SQL Queries** - Full control over database operations
- **Async/Await** - Asynchronous programming

## Dapper Implementation Details

### Why Dapper?
- **Performance** - Minimal overhead compared to full ORMs
- **Control** - Explicit SQL queries for full control
- **Simplicity** - Easy to understand and maintain
- **No Magic** - What you write is what executes

### Example SQL Queries in BookRepository

**Get All Books:**
```sql
SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
FROM Book
ORDER BY BookId
```

**Add Book with Identity Return:**
```sql
INSERT INTO Book (Title, Author, ISBN, PublicationYear, IsAvailable)
VALUES (@Title, @Author, @ISBN, @PublicationYear, @IsAvailable);

SELECT CAST(SCOPE_IDENTITY() AS INT);
```

**Search with LIKE:**
```sql
SELECT BookId, Title, Author, ISBN, PublicationYear, IsAvailable
FROM Book
WHERE Title LIKE '%' + @TitlePattern + '%'
ORDER BY Title
```

### Connection Factory Pattern
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
```

## Performance Considerations

1. **Async Operations** - All I/O operations use async/await
2. **Dapper Performance** - Near-ADO.NET speeds with object mapping
3. **Connection Pooling** - Enabled by default in SqlConnection
4. **Parameterized Queries** - Prevents SQL injection and improves performance
5. **Index on Title** - Improves search performance
6. **Efficient Algorithms** - Bitwise operations for power of two check
7. **Proper Disposal** - Using statements ensure connections are closed

## Security Considerations

1. **Input Validation** - Data annotations on DTOs
2. **SQL Injection Prevention** - EF Core parameterized queries
3. **HTTPS** - Configured by default
4. **Error Handling** - No sensitive info in error messages
5. **CORS** - Configured but can be restricted

## Conclusion

This implementation demonstrates:
- ✅ Clean, maintainable code
- ✅ SOLID principles throughout
- ✅ Proper separation of concerns
- ✅ Industry best practices
- ✅ Comprehensive documentation
- ✅ Extensible architecture
- ✅ Production-ready structure