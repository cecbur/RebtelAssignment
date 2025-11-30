

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

Interface to `BusinessLogic`

* `BusinessLogicContracts`
* `BusinessLogicGrpcClient`



### Database Layer

* `DataStorage`

Interface to `BusinessLogic`

* `DataStorageContracts`
* `DataStorageGrpcClient`


### Data Storage

use  MongoDB or SQL  for data persistence.

SQL Server is ust
Set up is done with scripts in folder `Database`


### Testing Requirements  

Emphasis on test coverage  

#### Unit Tests  for individual methods/classes 

Tests for `BusinessLogic.BookPatterns`, `BusinessLogic.PatronActivity` and `BusinessLogic.BorrowingPatterns` are found in  `BusinessLogicTests` 

#### Functional Tests  for key features 

Tests for `LibraryApi.Commands.AssignmentCommands` are found in  `LibraryApiTests.Commands`


#### Integration Tests  for database interactions 


TODO

                   ■   System Tests  to validate complete user flows 

           ○   Tests should be automated and easy to execute. 

### Code Quality  : 

Implemented especially along the path described in TODO

* Clean, maintainable, and well-structured code is highly valued.¨ 
* Clear separation of concerns across different layers. 
* Proper use of modern .NET patterns and best practices. 



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

TODO