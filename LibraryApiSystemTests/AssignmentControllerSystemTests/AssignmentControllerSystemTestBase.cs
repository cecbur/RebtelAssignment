using System.Diagnostics;
using System.Net;
using TestData;

namespace LibraryApiSystemTests.AssignmentControllerSystemTests;

/// <summary>
/// Base class for AssignmentController system tests.
/// Starts a single LibraryApi instance for all tests in the class, and cleans the database before each test.
/// </summary>
[NonParallelizable]
public abstract class AssignmentControllerSystemTestBase
{
    protected Process? _apiProcess;
    protected HttpClient _client = null!;
    protected TestDataGenerator _testData = null!;
    protected const string ApiBaseUrl = "http://localhost:7100";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Determine the correct path to LibraryApi project
        var solutionDir = TestHelpers.TryGetSolutionDirectory();
        if (solutionDir == null)
        {
            Assert.Fail("Solution directory not found. Tests require access to the solution structure.");
        }

        var libraryApiProject = Path.Combine(solutionDir!, "LibraryApi", "LibraryApi.csproj");

        // Start the LibraryApi process (once for all tests in this class)
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

    [SetUp]
    public async Task SetUp()
    {
        // Clean the database before each test
        await SqlServerTestFixture.CleanDatabase();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();

        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            _apiProcess.Kill(entireProcessTree: true);
            _apiProcess.WaitForExit(5000);
            _apiProcess.Dispose();
        }
    }
}
