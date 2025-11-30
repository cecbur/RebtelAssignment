using DataStorage;

namespace DataStorageIntegrationTests;

/// <summary>
/// Base class for repository integration tests.
/// Provides common setup for database cleanup, connection factory, and test data generation.
/// All repository integration tests should inherit from this class to ensure consistent setup.
/// </summary>
[TestFixture]
[NonParallelizable]
public abstract class RepositoryIntegrationTestBase<TRepository> where TRepository : class
{
    /// <summary>
    /// The System Under Test - the repository being tested.
    /// </summary>
    protected TRepository _sut = null!;

    /// <summary>
    /// Connection factory for creating database connections.
    /// Available to derived classes for creating additional repositories if needed.
    /// </summary>
    protected IDbConnectionFactory _connectionFactory = null!;

    /// <summary>
    /// Helper for generating test data (Authors, Books, Patrons, Loans).
    /// </summary>
    protected TestDataGenerator _testData = null!;

    /// <summary>
    /// Sets up the test environment before each test.
    /// Cleans the database, creates connection factory, and initializes the repository under test.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Clean the database before each test to ensure isolation
        await SqlServerTestFixture.CleanDatabase();

        // Create connection factory and test data generator
        _connectionFactory = new SqlServerConnectionFactory(SqlServerTestFixture.ConnectionString);
        _testData = new TestDataGenerator(SqlServerTestFixture.ConnectionString);

        // Let derived class create its specific repository
        _sut = CreateRepository();
    }

    /// <summary>
    /// Derived classes implement this to create their specific repository instance.
    /// Called after _connectionFactory is initialized, so it can be used in the implementation.
    /// </summary>
    /// <returns>The repository instance to test</returns>
    protected abstract TRepository CreateRepository();
}
