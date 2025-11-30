# Library API - Setup Instructions

This guide will walk you through setting up and running the Library API project on your local machine.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

1. **.NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Docker Desktop**
   - Download from: https://www.docker.com/products/docker-desktop
   - Verify installation: `docker --version`

3. **SQL Server Command Line Tools (sqlcmd)**
   - Download from: https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility
   - Verify installation: `sqlcmd -?`

### Optional Software

4. **Git** (if you want to clone the repository)
   - Download from: https://git-scm.com/downloads

---

## Step 1: Clone or Download the Project

If you have Git installed:
```bash
git clone <repository-url>
cd RebtelAssignment
```

Or download and extract the project ZIP file to your preferred location.

---

## Step 2: Set Up SQL Server in Docker

### Pull the SQL Server Docker Image

Open a terminal/command prompt and run:

```bash
docker pull mcr.microsoft.com/mssql/server:2025-latest
```

This downloads the SQL Server 2025 container image (~2GB).

### Create and Start the SQL Server Container

Run the following command to create a container named `sql1`:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Byt1.Byt2.Byt3" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2025-latest
```

**What this does:**
- Creates a container named `sql1`
- Sets the SA password to `Byt1.Byt2.Byt3`
- Exposes SQL Server on port 1433
- Runs the container in detached mode (background)

### Verify the Container is Running

```bash
docker ps
```

You should see a container named `sql1` with status `Up`.

---

## Step 3: Create and Populate the Database

Navigate to the project root directory (where `LibrarySolution.sln` is located), then:

### Windows
```bash
cd Database
SetupDatabase.bat
```

### Linux/macOS
```bash
cd Database
sqlcmd -S localhost,1433 -U sa -P "Byt1.Byt2.Byt3" -i DatabaseSetup.sql
```

**What this does:**
- Creates the `Library` database
- Creates tables: Author, Book, Patron, Loan
- Populates the database with sample data (16 books, authors, patrons, and loan records)

### Verify Database Setup

Check that the database was created successfully:

```bash
sqlcmd -S localhost,1433 -U sa -P "Byt1.Byt2.Byt3" -d Library -Q "SELECT COUNT(*) FROM Book"
```

You should see a count of 16 books.

---

## Step 4: Build the Solution

Return to the project root directory and build the solution:

```bash
dotnet build LibrarySolution.sln
```

This compiles all projects in the solution. You should see:
```
Build succeeded.
```

---

## Step 5: Run the Application

### Option 1: Run from LibraryApi Directory

```bash
cd LibraryApi
dotnet run
```

### Option 2: Run from Solution Root

```bash
dotnet run --project LibraryApi/LibraryApi.csproj
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

The API is now running! 🎉

---

## Step 6: Access the API

### Swagger UI (Recommended)

Open your browser and navigate to:
- **HTTPS:** https://localhost:7000
- **HTTP:** http://localhost:5000

Swagger UI will load automatically at the root URL, providing an interactive API documentation interface.

### Available Endpoints

The Assignment Controller provides these endpoints:

1. **GET /api/Assignment/most-loaned-books** - Get books sorted by loan count
2. **GET /api/Assignment/most-active-patrons** - Get patrons sorted by activity
3. **GET /api/Assignment/reading-pace-pages-per-day/{loanId}** - Get reading pace for a loan
4. **GET /api/Assignment/other-books-borrowed/{bookId}** - Get borrowing pattern associations

---

## Testing the API

### Using Swagger UI

1. Navigate to https://localhost:7000
2. Find an endpoint (e.g., `/api/Assignment/most-loaned-books`)
3. Click **"Try it out"**
4. Adjust parameters if needed
5. Click **"Execute"**
6. View the response below

### Using curl

#### Get Most Loaned Books
```bash
curl -X GET "https://localhost:7000/api/Assignment/most-loaned-books" -k
```

#### Get Most Active Patrons (within a date range)
```bash
curl -X GET "https://localhost:7000/api/Assignment/most-active-patrons?startDate=2024-01-01&endDate=2024-12-31&maxPatrons=10" -k
```

#### Get Reading Pace for a Loan
```bash
curl -X GET "https://localhost:7000/api/Assignment/reading-pace-pages-per-day/1" -k
```

#### Get Borrowing Patterns
```bash
curl -X GET "https://localhost:7000/api/Assignment/other-books-borrowed/1" -k
```

**Note:** The `-k` flag allows insecure HTTPS connections (self-signed certificate).

---

## Running Tests

The solution includes comprehensive tests:

### Run All Tests
```bash
dotnet test LibrarySolution.sln
```

### Run Specific Test Project

**Unit Tests:**
```bash
dotnet test BusinessLogicTests/BusinessLogicTests.csproj
```

**Functional Tests:**
```bash
dotnet test LibraryApiTests/LibraryApiTests.csproj
```

**Integration Tests:**
```bash
dotnet test DataStorageIntegrationTests/DataStorageIntegrationTests.csproj
```

**System Tests:**
```bash
dotnet test LibraryApiSystemTests/LibraryApiSystemTests.csproj
```

**Architecture Tests:**
```bash
dotnet test ArchitectureTests/ArchitectureTests.csproj
```

---

## Configuration

### Connection String

The default connection string is configured in `LibraryApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Library;User Id=sa;Password=Byt1.Byt2.Byt3;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

If you need to change the SQL Server password or connection details, update this file.

### Application Ports

The API ports are configured in `LibraryApi/Properties/launchSettings.json`:
- HTTP: 5000
- HTTPS: 7000

---

## Managing the Docker Container

### Stop the Container
```bash
docker stop sql1
```

### Start the Container (after stopping)
```bash
docker start sql1
```

### View Container Logs
```bash
docker logs sql1
```

### Remove the Container (deletes all data)
```bash
docker stop sql1
docker rm sql1
```

---

## Troubleshooting

### Issue: "sqlcmd is not found"

**Solution:** Install SQL Server Command Line Tools:
- **Windows:** Download from https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility
- **Linux:** `sudo apt-get install mssql-tools`
- **macOS:** `brew install sqlcmd`

---

### Issue: "SQL Server container 'sql1' is not running"

**Solution:** Start the container:
```bash
docker start sql1
```

Or create it if it doesn't exist:
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Byt1.Byt2.Byt3" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2025-latest
```

---

### Issue: "Port 1433 is already in use"

**Solution:** Another SQL Server instance is using port 1433. Either:

1. Stop the conflicting service
2. Use a different port:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Byt1.Byt2.Byt3" -p 1434:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2025-latest
   ```
   Then update the connection string in `appsettings.json` to use `localhost,1434`

---

### Issue: "Database 'Library' does not exist"

**Solution:** Run the database setup script:
```bash
cd Database
SetupDatabase.bat  # Windows
# OR
sqlcmd -S localhost,1433 -U sa -P "Byt1.Byt2.Byt3" -i DatabaseSetup.sql  # Linux/macOS
```

---

### Issue: "Login failed for user 'sa'"

**Solution:** Verify the password in your connection string matches the Docker container password (`Byt1.Byt2.Byt3`).

---

### Issue: "Port 7000 is already in use"

**Solution:** Another application is using port 7000. Change the ports in `LibraryApi/Properties/launchSettings.json`.

---

## Sample Data

The database includes sample data to test the API:

### Books (16 total)
- Classic Literature: "1984", "Pride and Prejudice", "Moby Dick", "To Kill a Mockingbird"
- Fantasy: "Harry Potter and the Sorcerer's Stone", "The Lord of the Rings", "The Hobbit"
- Science Fiction: "Brave New World", "Fahrenheit 451", "Dune"
- And more...

### Authors
Multiple authors including George Orwell, J.R.R. Tolkien, J.K. Rowling, Jane Austen, etc.

### Patrons
Sample library members with loan history

### Loans
Historical and active loan records to demonstrate borrowing patterns

---

## Next Steps

- **Explore the API:** Use Swagger UI to test all endpoints
- **Run the Tests:** Verify everything works with `dotnet test`
- **Read the Documentation:** Check `Documentation/Architecture.md` for architecture details
- **Customize:** Modify the code to add new features or endpoints

---

## Quick Reference

### Start Everything
```bash
# 1. Start Docker container
docker start sql1

# 2. Run the API
dotnet run --project LibraryApi/LibraryApi.csproj

# 3. Open browser to https://localhost:7000
```

### Stop Everything
```bash
# 1. Stop the API (Ctrl+C in the terminal)
# 2. Stop Docker container
docker stop sql1
```

---

## Support

If you encounter issues not covered in this guide, please contact Cecilia or check:
- Project documentation in the `Documentation/` folder
- Error logs in the console output when running the API
