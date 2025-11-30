using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using LibraryApi.DTOs;
using TestData;

namespace LibraryApiIntegrationTests.AssignmentControllerSystemTests;

/// <summary>
/// End-to-end HTTP integration tests for AssignmentController.
/// Uses real HTTP calls and a real SQL Server database (via Testcontainers).
/// Nothing is mocked - tests the full stack from HTTP request to database.
/// </summary>
[TestFixture]
[NonParallelizable]
public class GetBooksSortedByMostLoanedTests
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

        Console.WriteLine($"[TEST] Solution dir: {solutionDir}");
        Console.WriteLine($"[TEST] LibraryApi project: {libraryApiProject}");
        Console.WriteLine($"[TEST] Project exists: {File.Exists(libraryApiProject)}");

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

        // Capture output for debugging
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

        // Wait for the API to be ready (simple polling approach)
        _client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        var maxAttempts = 30;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await _client.GetAsync("/api/Assignment/most-loaned-books");
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    break; // API is responding
                }
            }
            catch
            {
                if (i == maxAttempts - 1) throw;
                await Task.Delay(1000);
            }
        }

        // Create test data generator
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
    public async Task GetBooksSortedByMostLoaned_WithMultipleBooksAndLoans_ReturnsBooksOrderedByLoanCount()
    {
        // Arrange - Create test data
        var author1 = await _testData.CreateAuthor("George", "Orwell");
        var author2 = await _testData.CreateAuthor("Aldous", "Huxley");
        var author3 = await _testData.CreateAuthor("Ray", "Bradbury");

        // Create books
        var book1984 = await _testData.CreateBook("1984", author1.Id, "978-0451524935", 1949, 328);
        var bookAnimalFarm = await _testData.CreateBook("Animal Farm", author1.Id, "978-0451526342", 1945, 112);
        var bookBraveNewWorld = await _testData.CreateBook("Brave New World", author2.Id, "978-0060850524", 1932, 268);
        var bookFahrenheit451 = await _testData.CreateBook("Fahrenheit 451", author3.Id, "978-1451673319", 1953, 249);

        // Create patrons
        var patron1 = await _testData.CreatePatron("Alice", "Johnson", "alice@test.com");
        var patron2 = await _testData.CreatePatron("Bob", "Smith", "bob@test.com");
        var patron3 = await _testData.CreatePatron("Carol", "Williams", "carol@test.com");

        var baseDate = new DateTime(2024, 1, 1);

        // Create loans with different frequencies:
        // - "1984" borrowed 3 times (most loaned)
        // - "Animal Farm" borrowed 2 times
        // - "Brave New World" borrowed 1 time
        // - "Fahrenheit 451" never borrowed
        await _testData.CreateLoans([
            // Patron 1 borrows 1984 and Animal Farm
            new TestDataGenerator.LoanData(book1984.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(bookAnimalFarm.Id, patron1.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),

            // Patron 2 borrows 1984 and Animal Farm
            new TestDataGenerator.LoanData(book1984.Id, patron2.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(bookAnimalFarm.Id, patron2.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),

            // Patron 3 borrows 1984 and Brave New World
            new TestDataGenerator.LoanData(book1984.Id, patron3.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(bookBraveNewWorld.Id, patron3.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true)
        ]);

        // Act - Call the HTTP endpoint
        var response = await _client.GetAsync("/api/Assignment/most-loaned-books");

        // Assert - Verify HTTP response
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "HTTP response should be 200 OK");

        var bookLoans = await response.Content.ReadFromJsonAsync<BookLoansResponse[]>();
        Assert.That(bookLoans, Is.Not.Null, "Response body should not be null");
        Assert.That(bookLoans, Has.Length.EqualTo(4), "Should return all 4 books");

        // Verify books are sorted by loan count (descending)
        Assert.That(bookLoans![0].Book.Title, Is.EqualTo( book1984.Title), "First book should be '1984' (3 loans)");
        Assert.That(bookLoans[0].LoanCount, Is.EqualTo(3), "'1984' should have 3 loans");

        Assert.That(bookLoans[1].Book.Title, Is.EqualTo(bookAnimalFarm.Title), "Second book should be 'Animal Farm' (2 loans)");
        Assert.That(bookLoans[1].LoanCount, Is.EqualTo(2), "'Animal Farm' should have 2 loans");

        Assert.That(bookLoans[2].Book.Title, Is.EqualTo(bookBraveNewWorld.Title), "Third book should be 'Brave New World' (1 loan)");
        Assert.That(bookLoans[2].LoanCount, Is.EqualTo(1), "'Brave New World' should have 1 loan");

        Assert.That(bookLoans[3].Book.Title, Is.EqualTo( bookFahrenheit451.Title), "Fourth book should be 'Fahrenheit 451' (0 loans)");
        Assert.That(bookLoans[3].LoanCount, Is.EqualTo(0), "'Fahrenheit 451' should have 0 loans");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithMaxBooksParameter_ReturnsLimitedResults()
    {
        // Arrange - Create test data with 3 books
        var author = await _testData.CreateAuthor("Test", "Author");
        var book1 = await _testData.CreateBook("Book 1", author.Id, "978-0000000001");
        var book2 = await _testData.CreateBook("Book 2", author.Id, "978-0000000002");
        var book3 = await _testData.CreateBook("Book 3", author.Id, "978-0000000003");

        var patron = await _testData.CreatePatron("Test", "Patron", "test@test.com");
        var baseDate = DateTime.Now;

        // Create loans: book1 has 3, book2 has 2, book3 has 1
        await _testData.CreateLoans([
            new TestDataGenerator.LoanData(book1.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book1.Id, patron.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(book1.Id, patron.Id, baseDate.AddDays(40), baseDate.AddDays(54), baseDate.AddDays(50), true),
            new TestDataGenerator.LoanData(book2.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true),
            new TestDataGenerator.LoanData(book2.Id, patron.Id, baseDate.AddDays(20), baseDate.AddDays(34), baseDate.AddDays(30), true),
            new TestDataGenerator.LoanData(book3.Id, patron.Id, baseDate, baseDate.AddDays(14), baseDate.AddDays(10), true)
        ]);

        // Act - Call endpoint with maxBooks=2
        var response = await _client.GetAsync("/api/Assignment/most-loaned-books?maxBooks=2");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var bookLoans = await response.Content.ReadFromJsonAsync<BookLoansResponse[]>();
        Assert.That(bookLoans, Has.Length.EqualTo(2), "Should return only 2 books when maxBooks=2");
        Assert.That(bookLoans![0].LoanCount, Is.EqualTo(3), "First book should have 3 loans");
        Assert.That(bookLoans[1].LoanCount, Is.EqualTo(2), "Second book should have 2 loans");
    }

    [Test]
    public async Task GetBooksSortedByMostLoaned_WithNoBooksInDatabase_ReturnsEmptyArray()
    {
        // Arrange - Empty database (already cleaned in SetUp)

        // Act
        var response = await _client.GetAsync("/api/Assignment/most-loaned-books");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var bookLoans = await response.Content.ReadFromJsonAsync<BookLoansResponse[]>();
        Assert.That(bookLoans, Is.Not.Null, "Response should not be null");
        Assert.That(bookLoans, Is.Empty, "Should return empty array when no books exist");
    }
}
