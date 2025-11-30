using Dapper;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Testcontainers.MsSql;

namespace TestData;

/// <summary>
/// Base test fixture that manages a SQL Server container lifecycle for integration tests.
/// The container is started once and reused across all tests in the fixture.
/// NOTE: Tests using this fixture should NOT run in parallel due to shared database state.
/// </summary>
[SetUpFixture]
public class SqlServerTestFixture
{
    private static MsSqlContainer? _msSqlContainer;
    public static string ConnectionString { get; private set; } = null!;

    /// <summary>
    /// Starts the SQL Server container and initializes the database schema.
    /// Called once before any tests run in the TestData namespace.
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await InitializeAsync();
    }

    /// <summary>
    /// Stops and disposes the SQL Server container.
    /// Called once after all tests complete in the TestData namespace.
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await DisposeAsync();
    }

    /// <summary>
    /// Starts the SQL Server container and initializes the database schema.
    /// This is the public method that can be called from other test fixtures.
    /// </summary>
    public static async Task InitializeAsync()
    {
        // Only initialize once (in case multiple test projects call this)
        if (_msSqlContainer != null)
        {
            return;
        }

        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .Build();

        // Start the SQL Server container
        await _msSqlContainer.StartAsync();

        ConnectionString = _msSqlContainer.GetConnectionString();

        // Initialize the database schema
        await InitializeDatabaseSchema();
    }

    /// <summary>
    /// Stops and disposes the SQL Server container.
    /// This is the public method that can be called from other test fixtures.
    /// </summary>
    public static async Task DisposeAsync()
    {
        if (_msSqlContainer != null)
        {
            await _msSqlContainer.DisposeAsync();
            _msSqlContainer = null;
        }
    }

    /// <summary>
    /// Cleans all data from test tables while preserving schema.
    /// Call this in test teardown to ensure test isolation.
    /// </summary>
    public static async Task CleanDatabase()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Delete in correct order to respect foreign key constraints
        await connection.ExecuteAsync("DELETE FROM [Loan]");
        await connection.ExecuteAsync("DELETE FROM [Book]");
        await connection.ExecuteAsync("DELETE FROM [Author]");
        await connection.ExecuteAsync("DELETE FROM [Patron]");
    }

    /// <summary>
    /// Initializes the database schema.
    /// NOTE: Schema is defined here to keep tests self-contained. Ideally, this would parse
    /// DatabaseSetup.sql, but SQL parsing is complex due to GO statements, comments, and dependencies.
    /// For now, the schema is duplicated here for simplicity and test reliability.
    /// </summary>
    private static async Task InitializeDatabaseSchema()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Create Author table
        await connection.ExecuteAsync(@"
            CREATE TABLE [dbo].[Author] (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [GivenName] NVARCHAR(100) NULL,
                [Surname] NVARCHAR(100) NOT NULL,
                CONSTRAINT [PK_Author] PRIMARY KEY CLUSTERED ([Id] ASC)
            );");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Author_Surname]
            ON [dbo].[Author] ([Surname] ASC);");

        // Create Book table
        await connection.ExecuteAsync(@"
            CREATE TABLE [dbo].[Book] (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [Title] NVARCHAR(200) NOT NULL,
                [AuthorId] INT NULL,
                [ISBN] NVARCHAR(20) NULL,
                [PublicationYear] INT NULL,
                [NumberOfPages] INT NULL,
                [IsAvailable] BIT NOT NULL DEFAULT 1,
                CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([Id] ASC),
                CONSTRAINT [UQ_Book_ISBN] UNIQUE NONCLUSTERED ([ISBN] ASC),
                CONSTRAINT [FK_Book_Author] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Author]([Id])
            );");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Book_Title]
            ON [dbo].[Book] ([Title] ASC);");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Book_AuthorId]
            ON [dbo].[Book] ([AuthorId] ASC);");

        // Create Patron table
        await connection.ExecuteAsync(@"
            CREATE TABLE [dbo].[Patron] (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [FirstName] NVARCHAR(100) NOT NULL,
                [LastName] NVARCHAR(100) NOT NULL,
                [Email] NVARCHAR(200) NOT NULL,
                [PhoneNumber] NVARCHAR(20) NULL,
                [MembershipDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
                [IsActive] BIT NOT NULL DEFAULT 1,
                CONSTRAINT [PK_Patron] PRIMARY KEY CLUSTERED ([Id] ASC),
                CONSTRAINT [UQ_Patron_Email] UNIQUE NONCLUSTERED ([Email] ASC)
            );");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Patron_LastName]
            ON [dbo].[Patron] ([LastName] ASC);");

        // Create Loan table
        await connection.ExecuteAsync(@"
            CREATE TABLE [dbo].[Loan] (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [BookId] INT NOT NULL,
                [PatronId] INT NOT NULL,
                [LoanDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
                [DueDate] DATETIME2 NOT NULL,
                [ReturnDate] DATETIME2 NULL,
                [IsReturned] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_Loan] PRIMARY KEY CLUSTERED ([Id] ASC),
                CONSTRAINT [FK_Loan_Book] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Book]([Id]),
                CONSTRAINT [FK_Loan_Patron] FOREIGN KEY ([PatronId]) REFERENCES [dbo].[Patron]([Id])
            );");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Loan_BookId]
            ON [dbo].[Loan] ([BookId] ASC);");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Loan_PatronId]
            ON [dbo].[Loan] ([PatronId] ASC);");

        await connection.ExecuteAsync(@"
            CREATE NONCLUSTERED INDEX [IX_Loan_IsReturned]
            ON [dbo].[Loan] ([IsReturned] ASC);");
    }
}
