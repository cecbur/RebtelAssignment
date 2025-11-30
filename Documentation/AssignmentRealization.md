

# Senior .NET Developer Assignment: 

Where to find the different parts

 ## Starters - Warm-up Tasks 

`BusinessLogic.AssignmentStarters`

### 1. Check if a Book ID is a Power of Two 

 Implement a method that checks whether a given  Book  ID  number is a power of 2. 

`BusinessLogic.AssignmentStarters.IsPowerOfTwo`

### 2. Reverse a Book Title 

Write a method that takes a  book title  as input and  returns it reversed. 
  (Example: "Moby Dick" → "kciD yboM") 

`BusinessLogic.AssignmentStarters.ReverseTitle`


### 3. Generate Book Title Replicas 

Create a method that  repeats a given book title  a  specified number of times. 
  (Example: ("Read", 3) → "ReadReadRead") 

`BusinessLogic.AssignmentStarters.GenerateTitleRepetitions`


### 4. List Odd-Numbered Book IDs 

Implement a method that prints all odd numbers between 0 and 100, simulating the generation of 
  odd-numbered IDs  for certain book collections or limited  editions. 

`BusinessLogic.AssignmentStarters.PrintOddNumberedBookIds0To100`

  
##  Main task - Library API System 
You are recommended to spend as little time as possible to solve the following problem. Ideally, it should take 5-8 hours for a person with the right competence. 


 Problem Statement 
A public library requires a robust API-driven application to manage its books, borrowers, and 
 lending activity. The goal is to provide librarians with reliable insights into inventory status and borrowing patterns, enabling them to optimize day-to-day operations and long-term planning. 
   The system should expose a set of HTTP API endpoints designed to answer key business 
    questions, such as: 




### Inventory Insights 

 What are the most borrowed books? 

`LibraryApi.Controllers.AssignmentController.GetBooksSortedByMostLoaned`

 User Activity 

Which users have borrowed the most books within a given time frame? 

`LibraryApi.Controllers.AssignmentController.GetMostActivePatrons`


 Estimate a user’s reading pace (pages per day) based on the borrow and return duration of a 
  book, assuming continuous reading. 

`LibraryApi.Controllers.AssignmentController.GetReadingPacePagesPerDay`


 Borrowing Patterns 
 What other books were borrowed by individuals who borrowed a specific book? 

`LibraryApi.Controllers.AssignmentController.GetOtherBooksBorrowed`


### Constraints & Assumptions 
  The application may be prepopulated with sample data for demonstration purposes. 

  Database setup can be found in The folder `Database`

 The API will not be publicly accessible, so security concerns are out of scope for this 
  assignment. 
   User management (e.g., authentication, authorization) is not considered part of the assignment. 
##  Expectations 
   The application should be  structured cleanly  into  at least two layers: 
   
   ### API Layer  : Handles HTTP interactions. 

Projects

* `LibraryApi`

### Service Layer  
Encapsulates business logic and database  operations.

* `BusinessLogic`
* `BusinessModels`

Interface to BusinessLogic

* `BusinessLogicContracts`
* `BusinessLogicGrpcClient`



### Database Layer

* `DataStorage`

Interface to DataStorage

* `DataStorageContracts`
* `DataStorageGrpcClient`


### Data Storage

use  MongoDB or SQL  for data persistence.

SQL Server is used

Set up is done with scripts in folder *Database*. See the instruction file *Setup.md* in folder *Documentation* for further instructions


### Testing Requirements  

Emphasis on test coverage  
Tests should be automated and easy to execute.

#### Unit Tests  for individual methods/classes 

Tests for `BusinessLogic.BookPatterns`, `BusinessLogic.PatronActivity` and `BusinessLogic.BorrowingPatterns` are found in  `BusinessLogicTests` 

#### Functional Tests  for key features 

Tests for `LibraryApi.Commands.AssignmentCommands` are found in  `LibraryApiTests.Commands`


#### Integration Tests  for database interactions 

Tests for `LoanRepository` and `BorrowingPatternRepository` are found in  `DataStorageIntegrationTests`


#### System Tests  to validate complete user flows 

Tests for `LibraryApi.Controllers.AssignmentController` are found in  `LibraryApiSystemTests.AssignmentControllerSystemTest`


    

### Code Quality  : 

Implementation has especially high quality along the path described in the file `ARCHITECTURE.md` in the folder `Documentation`

* Clean, maintainable, and well-structured code is highly valued.¨ 
* Clear separation of concerns across different layers. 
* Proper use of modern .NET patterns and best practices. 

**SOLID Principles applied in this solution**

1. ***Single Responsibility Principle (SRP)***
   - Each class has one reason to change
   - Separate projects for business logic, data access, and API

2. ***Open/Closed Principle (OCP)***
   - Interfaces allow extension without modification
   - Repository pattern enables easy data source changes

3. ***Liskov Substitution Principle (LSP)***
   - All implementations can replace their interfaces
   - Consistent behavior across implementations

4. ***Interface Segregation Principle (ISP)***
   - Small, focused interfaces
   - Clients depend only on methods they use

5. ***Dependency Inversion Principle (DIP)***
   - High-level modules depend on abstractions
   - Dependency injection throughout the solution


**Design Patterns Used**

- **Repository Pattern** - Abstracts data access logic
- **Factory Pattern** - Database connection factory (IDbConnectionFactory)
- **Dependency Injection** - Manages object dependencies
- **DTO Pattern** - Separates API models from domain models
- **POCO Pattern** - Plain objects without ORM dependencies

**Best Practices Implemented**

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
11. **Swagger/OpenAPI** - Interactive API documentation



###  Technologies 
#### Primary Language  : C#, .NET Core 

Implementded in C#, .NET Core 8

#### Database  : MongoDB or SQL 

Using SQL Server in a Docker container

#### Mandatory  : Usage of gRPC for internal service communication 

gRPC is used for communication with the API and for communication with DataStorage

#### Testing Frameworks  : NUnit, xUnit, or similar 

Tests are implemented in NUnit


### Submission should include a  README  file explaining  how to run the project and the 

There is a README.md file in the root directory. There are more .md files in the folder `Documentation`. README.md contains an overview of the documentation.
