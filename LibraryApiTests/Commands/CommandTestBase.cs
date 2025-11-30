using BusinessLogicContracts.Interfaces;
using BusinessLogicGrpcClient;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

/// <summary>
/// Generic base class for command tests that provides common setup, teardown, and constructor tests.
/// Eliminates boilerplate code across all command test classes.
/// </summary>
/// <typeparam name="TCommand">The type of command being tested</typeparam>
public abstract class CommandTestBase<TCommand> : DataStorageMockGrpcTestFixtureBase
    where TCommand : class
{
    protected Mock<ILogger<TCommand>> MockCommandLogger { get; private set; } = null!;
    protected TCommand Sut { get; private set; } = null!;
    protected IBusinessLogicFacade BusinessLogicFacade { get; private set; } = null!;
    protected TestDataBuilder TestDataBuilder { get; private set; } = null!;

    [SetUp]
    public async Task CommandSetUp()
    {
        TestDataBuilder = new TestDataBuilder();
        await SetUpGrpcServer();
        MockCommandLogger = new Mock<ILogger<TCommand>>();
        BusinessLogicFacade = new BusinessLogicGrpcFacade(ServerAddress);
        Sut = CreateSystemUnderTest(BusinessLogicFacade, MockCommandLogger.Object);
    }

    /// <summary>
    /// Factory method to create the system under test (SUT).
    /// Each derived test class must implement this to instantiate their specific command.
    /// </summary>
    protected abstract TCommand CreateSystemUnderTest(
        IBusinessLogicFacade businessLogicFacade,
        ILogger<TCommand> logger);

    [TearDown]
    public async Task CommandTearDown()
    {
        await TearDownGrpcServer();
    }

    [Test]
    public void Constructor_WithNullBusinessLogicFacade_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            CreateSystemUnderTest(null!, MockCommandLogger.Object),
            "Constructor should throw ArgumentNullException when businessLogicFacade is null");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            CreateSystemUnderTest(BusinessLogicFacade, null!),
            "Constructor should throw ArgumentNullException when logger is null");
    }
}
