using NUnit.Framework;

namespace LibraryApiSystemTests;

/// <summary>
/// SetUpFixture for LibraryApiSystemTests namespace.
/// Delegates to the shared SqlServerTestFixture in TestData project to start the SQL Server container.
/// </summary>
[SetUpFixture]
public class LibraryApiSystemTestsSetup
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
