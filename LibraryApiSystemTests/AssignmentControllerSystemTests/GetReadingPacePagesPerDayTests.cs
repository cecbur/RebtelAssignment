using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using LibraryApi.DTOs;
using TestData;

namespace LibraryApiSystemTests.AssignmentControllerSystemTests;

/// <summary>
/// End-to-end HTTP system tests for GetReadingPacePagesPerDay endpoint.
/// Uses real HTTP calls and a real SQL Server database (via Testcontainers).
/// Nothing is mocked - tests the full stack from HTTP request to database.
/// </summary>
[TestFixture]
[NonParallelizable]
public class GetReadingPacePagesPerDayTests
{
    private Process? _apiProcess;
    private HttpClient _client = null!;
    private TestDataGenerator _testData = null!;
    private const string ApiBaseUrl = "http://localhost:7100";

    [SetUp]
    public async Task SetUp()
    {
        // Clean the database before each test
        await SqlServerTestFixture.CleanDatabase();

        // Determine the correct path to LibraryApi project
        var solutionDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".."));
        var libraryApiProject = Path.Combine(solutionDir, "LibraryApi", "LibraryApi.csproj");

        // Start the LibraryApi process
        _apiProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{libraryApiProject}\" --no-launch-profile",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = solutionDir,
                Environment =
                {
                    ["ConnectionStrings__DefaultConnection"] = SqlServerTestFixture.ConnectionString,
                    ["Kestrel__HttpPort"] = "7100",
                    ["Kestrel__GrpcPort"] = "5100",
                    ["GrpcServer__Address"] = "http://localhost:5100"
                }
            }
        };

        _apiProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[API OUT] {e.Data}");
            }
        };
        _apiProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[API ERR] {e.Data}");
            }
        };

        _apiProcess.Start();
        _apiProcess.BeginOutputReadLine();
        _apiProcess.BeginErrorReadLine();

        // Wait for the API to be ready
        _client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        var maxAttempts = 30;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await _client.GetAsync("/api/Assignment/most-loaned-books");
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    break;
                }
            }
            catch
            {
                if (i == maxAttempts - 1) throw;
                await Task.Delay(1000);
            }
        }

        _testData = new TestDataGenerator(SqlServerTestFixture.ConnectionString);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();

        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            _apiProcess.Kill(entireProcessTree: true);
            _apiProcess.WaitForExit(5000);
            _apiProcess.Dispose();
        }
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithReturnedLoan_ReturnsCalculatedPace()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-0000000001", 1950, 300); // 300 pages
        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");

        var borrowDate = new DateTime(2024, 1, 1);
        var returnDate = new DateTime(2024, 1, 16); // 15 days
        var actualReturnDate = new DateTime(2024, 1, 16); // Returned on time

        var loans = await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, borrowDate, returnDate, actualReturnDate, true)
        ]);
        var loanId = loans[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{loanId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "HTTP response should be 200 OK");

        var paceResponse = await response.Content.ReadFromJsonAsync<LoanReadingPaceResponse>();
        Assert.That(paceResponse, Is.Not.Null, "Response body should not be null");
        Assert.That(paceResponse!.LoanId, Is.EqualTo(loanId), "LoanId should match");
        Assert.That(paceResponse.PagesPerDay, Is.Not.Null, "PagesPerDay should not be null for returned loan");
        Assert.That(paceResponse.PagesPerDay, Is.EqualTo(20.0).Within(0.01), "Reading pace should be 300 pages / 15 days = 20 pages/day");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithUnreturnedLoan_ReturnsNullPace()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-0000000001", 1950, 400);
        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");

        var borrowDate = new DateTime(2024, 1, 1);
        var dueDate = new DateTime(2024, 1, 15);

        var loans = await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, borrowDate, dueDate, null, false) // Not returned yet
        ]);
        var loanId = loans[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{loanId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paceResponse = await response.Content.ReadFromJsonAsync<LoanReadingPaceResponse>();
        Assert.That(paceResponse, Is.Not.Null);
        Assert.That(paceResponse!.LoanId, Is.EqualTo(loanId));
        Assert.That(paceResponse.PagesPerDay, Is.Null, "PagesPerDay should be null for unreturned loan");
        Assert.That(paceResponse.Message, Is.Not.Null.And.Not.Empty, "Should include a message explaining why pace is null");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithSameDayReturn_CalculatesPaceForOneDay()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Quick Read", author.Id, "978-0000000001", 1950, 100); // 100 pages
        var patron = await _testData.CreatePatron("Fast", "Reader", "fast@test.com");

        var borrowDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var returnDate = new DateTime(2024, 1, 1, 18, 0, 0); // Same day

        var loans = await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, borrowDate, returnDate, returnDate, true)
        ]);
        var loanId = loans[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{loanId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paceResponse = await response.Content.ReadFromJsonAsync<LoanReadingPaceResponse>();
        Assert.That(paceResponse, Is.Not.Null);
        Assert.That(paceResponse!.PagesPerDay, Is.Not.Null, "Should calculate pace even for same-day return");
        // For same-day returns, should treat as 1 day minimum
        Assert.That(paceResponse.PagesPerDay, Is.GreaterThan(0), "Pages per day should be positive");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithLongReadingPeriod_CalculatesCorrectPace()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Epic", "Writer");
        var book = await _testData.CreateBook("Long Novel", author.Id, "978-0000000001", 1950, 1200); // 1200 pages
        var patron = await _testData.CreatePatron("Slow", "Reader", "slow@test.com");

        var borrowDate = new DateTime(2024, 3, 1);
        var returnDate = new DateTime(2024, 4, 30); // 60 days

        var loans = await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, borrowDate, returnDate, returnDate, true)
        ]);
        var loanId = loans[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{loanId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paceResponse = await response.Content.ReadFromJsonAsync<LoanReadingPaceResponse>();
        Assert.That(paceResponse, Is.Not.Null);
        Assert.That(paceResponse!.PagesPerDay, Is.EqualTo(20.0).Within(0.01), "Reading pace should be 1200 pages / 60 days = 20 pages/day");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithNonExistentLoanId_ReturnsInternalServerError()
    {
        // Arrange
        var nonExistentLoanId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{nonExistentLoanId}");

        // Assert
        // The endpoint should return 500 Internal Server Error for non-existent loan
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError),
            "Should return 500 for non-existent loan ID");
    }

    [Test]
    public async Task GetReadingPacePagesPerDay_WithLateReturn_CalculatesBasedOnActualReturnDate()
    {
        // Arrange
        var author = await _testData.CreateAuthor("Test", "Author");
        var book = await _testData.CreateBook("Test Book", author.Id, "978-0000000001", 1950, 200);
        var patron = await _testData.CreatePatron("Late", "Returner", "late@test.com");

        var borrowDate = new DateTime(2024, 1, 1);
        var dueDate = new DateTime(2024, 1, 15); // Due in 14 days
        var actualReturnDate = new DateTime(2024, 1, 21); // Actually returned 6 days late (20 days total)

        var loans = await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book.Id, patron.Id, borrowDate, dueDate, actualReturnDate, true)
        ]);
        var loanId = loans[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/Assignment/reading-pace-pages-per-day/{loanId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paceResponse = await response.Content.ReadFromJsonAsync<LoanReadingPaceResponse>();
        Assert.That(paceResponse, Is.Not.Null);
        Assert.That(paceResponse!.PagesPerDay, Is.EqualTo(10.0).Within(0.01),
            "Reading pace should be based on actual return date: 200 pages / 20 days = 10 pages/day");
    }
}
