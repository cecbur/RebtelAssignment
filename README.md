

# README

## Purpose

This repository contains my implementation of the **Senior .NET Developer Assignment**, which includes both the warm-up exercises and the main Library API system.  
The goal of the solution is to demonstrate clear structure, testability, and maintainable design while meeting all functional requirements of the assignment.

I approached the problem by splitting responsibilities into clear layers, using gRPC for internal communication, and ensuring each feature is covered by appropriate tests (unit, functional, integration and system). The result is a small but complete system that models book inventory, borrowing activity and the insights required by the assignment.

The implementation also includes the four warm-up tasks:  
- Checking whether a book ID is a power of two  
- Reversing a book title  
- Generating repeated book titles  
- Listing odd-numbered book IDs  

## Documentation

All project documentation is located in the **Documentation** folder:

- **Architecture.md** — Overview of the system design, data flow, components and reasoning behind key choices.  
- **AssignmentRealization.md** — Mapping from assignment requirements to the exact files, classes, and methods where each part is implemented.  
- **Setup.md** — Instructions for running the application, executing the full test suite and configuring dependencies.  
- **senior-.net-developer-assignment-.pdf** — Original assignment description.

If you want to dive directly into how specific features were implemented, start with **AssignmentRealization.md**. If you want to understand the structure and design rationale, begin with **Architecture.md**.

## Running the Project and Tests

Setup, execution steps and test instructions are fully described in **Documentation/Setup.md**.  
Follow those instructions to run the API, internal services and the full test suite.




