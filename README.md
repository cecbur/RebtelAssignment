# Library Management System - .NET 8 Solution

A well-encapsulated .NET 8 solution following **Clean Code**, **SOLID principles**, and industry best practices. This solution provides HTTP APIs for library management with no frontend interface.

## Architecture Overview

The solution is structured into three separate projects following clean architecture principles:

### 1. **BusinessLogic** (Class Library)
Contains the core business logic and domain services.

**Key Components:**
- `IBookService` - Interface defining book-related business operations
- `BookService` - Implementation with 4 core methods:
  1. `IsPowerOfTwo(int bookId)` - Checks if a Book ID is a power of two
  2. `ReverseTitle(string title)` - Reverses a book title
  3. `GenerateTitleReplicas(string title, int count)` - Repeats a book title
  4. `GetOddNumberedBookIds(IEnumerable<int> bookIds)` - Lists odd-numbered Book IDs

### 2. **DataStorage** (Class Library)
Handles all database operations using **Dapper** with explicit SQL queries.

**Key Components:**
- `Book` - Plain POCO entity (no ORM dependencies)
- `IDbConnectionFactory` - Connection factory interface
- `SqlConnectionFactory` - SQL Server connection factory implementation
- `IBookRepository` - Repository interface for data access
- `BookRepository` - Implementation with explicit SQL queries using Dapper

### 3. **LibraryApi** (ASP.NET Core Web API)
Provides HTTP endpoints for accessing business logic and data.

**API Endpoints:**
- **BorrowingPatternsController**
  - `GET /api/BorrowingPatterns/analyze` - Analyzes borrowing patterns
  - `GET /api/BorrowingPatterns/is-power-of-two/{bookId}` - Checks if ID is power of two
  - `GET /api/BorrowingPatterns/odd-book-ids` - Gets odd-numbered book IDs

- **BookController**
  - `GET /api/Book` - Gets all books
  - `GET /api/Book/{id}` - Gets book by ID
  - `GET /api/Book/search?titlePattern=query` - Searches books by title
  - `POST /api/Book` - Creates a new book
  - `PUT /api/Book/{id}` - Updates a book
  - `DELETE /api/Book/{id}` - Deletes a book
  - `POST /api/Book/reverse-title` - Reverses a book title
  - `POST /api/Book/generate-replicas` - Generates title replicas

## SOLID Principles Applied

1. **Single Responsibility Principle (SRP)**
   - Each class has one reason to change
   - Separate projects for business logic, data access, and API

2. **Open/Closed Principle (OCP)**
   - Interfaces allow extension without modification
   - Repository pattern enables easy data source changes

3. **Liskov Substitution Principle (LSP)**
   - All implementations can replace their interfaces
   - Consistent behavior across implementations

4. **Interface Segregation Principle (ISP)**
   - Small, focused interfaces
   - Clients depend only on methods they use

5. **Dependency Inversion Principle (DIP)**
   - High-level modules depend on abstractions
   - Dependency injection throughout the solution

## Technology Stack

- **.NET 8** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API
- **Dapper** - Lightweight micro-ORM for high performance
- **SQL Server LocalDB** - Development database
- **Explicit SQL Queries** - Full control over database operations

## Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server LocalDB** - Included with Visual Studio or [download separately](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- **sqlcmd** utility - For running database setup script

## Database Setup

### Option 1: Automated Setup (Windows)
Run the provided batch script:
```bash
SetupDatabase.bat
```

### Option 2: Manual Setup
1. Ensure SQL Server LocalDB is running
2. Execute the SQL script:
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i DatabaseSetup.sql
```

### Verify Database Creation
```sql
sqlcmd -S "(localdb)\mssqllocaldb" -d Library -Q "SELECT COUNT(*) FROM Book"
```

## Running the Application

### 1. Build the Solution
```bash
dotnet build LibrarySolution.sln
```

### 2. Run the API
```bash
cd LibraryApi
dotnet run
```

### 3. Access Swagger UI
The application will start and display URLs. Navigate to:
- **HTTPS:** `https://localhost:7000` (or port shown in console)
- **HTTP:** `http://localhost:5000` (or port shown in console)

Swagger UI is configured to load at the root URL for easy API exploration.

## Connection String

The default connection string is in `LibraryApi/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Library;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

## API Testing Examples

### Using Swagger UI
1. Navigate to the application root URL
2. Expand any endpoint
3. Click "Try it out"
4. Enter parameters and click "Execute"

### Using curl

#### Analyze Borrowing Patterns
```bash
curl -X GET "https://localhost:7000/api/BorrowingPatterns/analyze"
```

#### Check if Book ID is Power of Two
```bash
curl -X GET "https://localhost:7000/api/BorrowingPatterns/is-power-of-two/8"
```

#### Get All Books
```bash
curl -X GET "https://localhost:7000/api/Book"
```

#### Reverse a Title
```bash
curl -X POST "https://localhost:7000/api/Book/reverse-title" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Moby Dick\"}"
```

#### Generate Title Replicas
```bash
curl -X POST "https://localhost:7000/api/Book/generate-replicas" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Read\",\"count\":3}"
```

## Project Structure

```
RebtelAssignment/
├── BusinessLogic/
│   ├── IBookService.cs
│   └── BookService.cs
├── DataStorage/
│   ├── Entities/
│   │   └── Book.cs
│   ├── Repositories/
│   │   ├── IBookRepository.cs
│   │   └── BookRepository.cs
│   ├── IDbConnectionFactory.cs
│   └── SqlConnectionFactory.cs
├── LibraryApi/
│   ├── Controllers/
│   │   ├── BorrowingPatternsController.cs
│   │   └── BookController.cs
│   ├── DTOs/
│   │   ├── BookDto.cs
│   │   ├── BorrowingPatternsResponse.cs
│   │   └── TitleOperationRequest.cs
│   ├── Program.cs
│   └── appsettings.json
├── DatabaseSetup.sql
├── SetupDatabase.bat
├── LibrarySolution.sln
└── README.md
```

## Design Patterns Used

- **Repository Pattern** - Abstracts data access logic
- **Factory Pattern** - Database connection factory (IDbConnectionFactory)
- **Dependency Injection** - Manages object dependencies
- **DTO Pattern** - Separates API models from domain models
- **POCO Pattern** - Plain objects without ORM dependencies

## Best Practices Implemented

1. **Async/Await** - All I/O operations are asynchronous
2. **Explicit SQL Queries** - Full control over database operations with Dapper
3. **Parameterized Queries** - Prevents SQL injection attacks
4. **Connection Management** - Proper using statements for connection disposal
5. **Logging** - Comprehensive logging throughout
6. **Error Handling** - Try-catch blocks with proper error responses
7. **Input Validation** - Data annotations and model validation
8. **XML Documentation** - Complete API documentation
9. **Separation of Concerns** - Clear boundaries between layers
10. **Naming Conventions** - Consistent and meaningful names
11. **CORS Support** - Configured for frontend integration
12. **Swagger/OpenAPI** - Interactive API documentation

## Sample Data

The database setup script includes 16 sample books:
- Classic literature (Moby Dick, 1984, Pride and Prejudice, etc.)
- Fantasy (The Hobbit, Harry Potter, The Lord of the Rings)
- Various publication years and authors

## Troubleshooting

### LocalDB Connection Issues
```bash
# Check if LocalDB is running
sqllocaldb info mssqllocaldb

# Start LocalDB if needed
sqllocaldb start mssqllocaldb
```

### Port Already in Use
Modify `LibraryApi/Properties/launchSettings.json` to change ports.

### Database Not Found
Ensure you've run the `DatabaseSetup.sql` script successfully.

## Future Enhancements

- Authentication & Authorization
- Caching layer (Redis)
- Unit and integration tests
- Docker containerization
- CI/CD pipeline
- API versioning
- Rate limiting

## License

This project is created for assessment purposes.