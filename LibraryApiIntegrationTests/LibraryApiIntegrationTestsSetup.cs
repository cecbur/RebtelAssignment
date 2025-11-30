using NUnit.Framework;

namespace LibraryApiIntegrationTests;

/// <summary>
/// SetUpFixture for LibraryApiIntegrationTests namespace.
/// Delegates to the shared SqlServerTestFixture in TestData project to start the SQL Server container.
/// </summary>
[SetUpFixture]
public class LibraryApiIntegrationTestsSetup
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await TestData.SqlServerTestFixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await TestData.SqlServerTestFixture.DisposeAsync();
    }
}
