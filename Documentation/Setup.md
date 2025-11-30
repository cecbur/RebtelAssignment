






## Prerequisites

Before starting the project a database is required. Follow these steps to create it.

### 1. Docker

Install Docker Desktop from https://www.docker.com/

### 2. Database container

Pull a Docker container with SQL Server. <br>
(Adapted from https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver17&tabs=cli&pivots=cs1-bash)

docker pull mcr.microsoft.com/mssql/server:2025-latest

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Byt1.Byt2.Byt3" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2025-latest

### 3. Database content

Run Database/SetupDatabase.bat to create database with test data.









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
