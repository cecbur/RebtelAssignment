namespace DataStorageIntegrationTests;

/// <summary>
/// SetUpFixture for DataStorageIntegrationTests namespace.
/// Delegates to the shared SqlServerTestFixture in TestData project.
/// </summary>
[SetUpFixture]
public class DataStorageIntegrationTestsSetup
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
