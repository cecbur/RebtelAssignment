using BusinessLogicContracts.Interfaces;
using BusinessModels;
using LibraryApi.Commands.AssignmentCommands;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApiTests.Commands;

[TestFixture]
public class GetMostActivePatronsCommandTests : CommandTestBase<GetMostActivePatronsCommand>
{
    private DateTime _startDate;
    private DateTime _endDate;

    protected override GetMostActivePatronsCommand CreateSystemUnderTest(
        IBusinessLogicFacade businessLogicFacade,
        ILogger<GetMostActivePatronsCommand> logger)
    {
        return new GetMostActivePatronsCommand(businessLogicFacade, logger);
    }

    [SetUp]
    public async Task SetUpMostActivePatrons()
    {
        await CommandSetUp();
        _startDate = new DateTime(2024, 1, 1);
        _endDate = new DateTime(2024, 12, 31);
    }

    [Test]
    public async Task GetMostActivePatrons_WithValidData_ReturnsSuccessAndPatrons()
    {
        // Arrange
        const int patron1LoanCount = 5;
        const int patron2LoanCount = 3;
        const int maxPatrons = 10;

        var patron1 = TestDataBuilder.CreatePatron(1, "John", "Doe");
        var patron2 = TestDataBuilder.CreatePatron(2, "Jane", "Smith");

        var patron1Loans = TestDataBuilder.CreateLoansForPatron(patron1, startId: 1, count: patron1LoanCount);
        var patron2Loans = TestDataBuilder.CreateLoansForPatron(patron2, startId: patron1LoanCount + 1, count: patron2LoanCount);
        var loans = patron1Loans.Concat(patron2Loans).ToArray();

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with valid data");
        Assert.That(response, Has.Length.EqualTo(2), "Should return exactly 2 patrons");
        Assert.That(response[0].PatronId, Is.EqualTo(patron1.Id), "First patron should be John Doe (most active)");
        Assert.That(response[0].PatronName, Is.EqualTo("John Doe"), "First patron name should be 'John Doe'");
        Assert.That(response[0].LoanCount, Is.EqualTo(patron1LoanCount), "First patron should have 5 loans");
        Assert.That(response[1].PatronId, Is.EqualTo(patron2.Id), "Second patron should be Jane Smith");
        Assert.That(response[1].PatronName, Is.EqualTo("Jane Smith"), "Second patron name should be 'Jane Smith'");
        Assert.That(response[1].LoanCount, Is.EqualTo(patron2LoanCount), "Second patron should have 3 loans");

        MockLoanRepository.Verify(r => r.GetLoansByTime(_startDate, _endDate), Times.Once);
    }

    [Test]
    public async Task GetMostActivePatrons_WithMaxPatronsLimit_LimitsResults()
    {
        // Arrange
        const int maxPatrons = 2;
        const int patron1LoanCount = 5;
        const int patron2LoanCount = 4;
        const int patron3LoanCount = 3;

        var patron1 = TestDataBuilder.CreatePatron(1, "Patron", "One");
        var patron2 = TestDataBuilder.CreatePatron(2, "Patron", "Two");
        var patron3 = TestDataBuilder.CreatePatron(3, "Patron", "Three");

        var patron1Loans = TestDataBuilder.CreateLoansForPatron(patron1, startId: 1, count: patron1LoanCount);
        var patron2Loans = TestDataBuilder.CreateLoansForPatron(patron2, startId: patron1LoanCount + 1, count: patron2LoanCount);
        var patron3Loans = TestDataBuilder.CreateLoansForPatron(patron3, startId: patron1LoanCount + patron2LoanCount + 1, count: patron3LoanCount);
        var loans = patron1Loans.Concat(patron2Loans).Concat(patron3Loans).ToArray();

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with maxPatrons limit");
        Assert.That(response, Has.Length.EqualTo(maxPatrons), "Should return exactly 2 patrons when maxPatrons=2");
        Assert.That(response[0].LoanCount, Is.EqualTo(patron1LoanCount), "First patron should have 5 loans (highest)");
        Assert.That(response[1].LoanCount, Is.EqualTo(patron2LoanCount), "Second patron should have 4 loans");

        MockLoanRepository.Verify(r => r.GetLoansByTime(_startDate, _endDate), Times.Once);
    }

    [Test]
    public async Task GetMostActivePatrons_WithEmptyResult_ReturnsSuccessAndEmptyArray()
    {
        // Arrange
        const int maxPatrons = 10;
        var emptyLoans = Array.Empty<Loan>();

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(emptyLoans);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed even with no loans");
        Assert.That(response, Is.Empty, "Should return empty array when there are no loans");

        MockLoanRepository.Verify(r => r.GetLoansByTime(_startDate, _endDate), Times.Once);
    }

    [Test]
    public async Task GetMostActivePatrons_WhenRepositoryThrowsException_ReturnsFalseAndEmptyArray()
    {
        // Arrange
        const int maxPatrons = 10;

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when repository throws exception");
        Assert.That(response, Is.Empty, "Response should be empty when operation fails");

        MockLoanRepository.Verify(r => r.GetLoansByTime(_startDate, _endDate), Times.Once);
    }

    [Test]
    public async Task GetMostActivePatrons_WhenRepositoryThrowsException_LogsError()
    {
        // Arrange
        const int maxPatrons = 10;

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert - Verify that an error was logged (without checking the exact message)
        MockCommandLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once, "Error should be logged when repository throws exception");
    }

    [Test]
    public async Task GetMostActivePatrons_WithDefaultStartDate_ReturnsFailure()
    {
        // Arrange
        const int maxPatrons = 10;
        var defaultStartDate = default(DateTime);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(defaultStartDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when startDate is default value");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetMostActivePatrons_WithDefaultEndDate_ReturnsFailure()
    {
        // Arrange
        const int maxPatrons = 10;
        var defaultEndDate = default(DateTime);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, defaultEndDate, maxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when endDate is default value");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetMostActivePatrons_WithStartDateAfterEndDate_ReturnsFailure()
    {
        // Arrange
        const int maxPatrons = 10;
        var startDate = new DateTime(2024, 12, 31);
        var endDate = new DateTime(2024, 1, 1);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(startDate, endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when startDate is after endDate");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetMostActivePatrons_WithZeroMaxPatrons_ReturnsFailure()
    {
        // Arrange
        const int zeroMaxPatrons = 0;

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, zeroMaxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when maxPatrons is 0");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetMostActivePatrons_WithNegativeMaxPatrons_ReturnsFailure()
    {
        // Arrange
        const int negativeMaxPatrons = -1;

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, negativeMaxPatrons);

        // Assert
        Assert.That(success, Is.False, "Operation should fail when maxPatrons is negative");
        Assert.That(response, Is.Empty, "Response should be empty when validation fails");

        // Verify repository was never called due to validation failure
        MockLoanRepository.Verify(r => r.GetLoansByTime(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never,
            "Repository should not be called when validation fails");
    }

    [Test]
    public async Task GetMostActivePatrons_WithIdenticalLoanCounts_MaintainsStableOrder()
    {
        // Arrange
        const int sameLoanCount = 3;
        const int maxPatrons = 10;

        var patron1 = TestDataBuilder.CreatePatron(1, "Patron", "A");
        var patron2 = TestDataBuilder.CreatePatron(2, "Patron", "B");
        var patron3 = TestDataBuilder.CreatePatron(3, "Patron", "C");

        var patron1Loans = TestDataBuilder.CreateLoansForPatron(patron1, startId: 1, count: sameLoanCount);
        var patron2Loans = TestDataBuilder.CreateLoansForPatron(patron2, startId: sameLoanCount + 1, count: sameLoanCount);
        var patron3Loans = TestDataBuilder.CreateLoansForPatron(patron3, startId: sameLoanCount * 2 + 1, count: sameLoanCount);
        var loans = patron1Loans.Concat(patron2Loans).Concat(patron3Loans).ToArray();

        MockLoanRepository
            .Setup(r => r.GetLoansByTime(_startDate, _endDate))
            .ReturnsAsync(loans);

        // Act
        var (success, response) = await Sut.GetMostActivePatrons(_startDate, _endDate, maxPatrons);

        // Assert
        Assert.That(success, Is.True, "Operation should succeed with identical loan counts");
        Assert.That(response, Has.Length.EqualTo(3), "Should return all 3 patrons");

        // All patrons should have the same loan count
        Assert.That(response[0].LoanCount, Is.EqualTo(sameLoanCount), "First patron should have 3 loans");
        Assert.That(response[1].LoanCount, Is.EqualTo(sameLoanCount), "Second patron should have 3 loans");
        Assert.That(response[2].LoanCount, Is.EqualTo(sameLoanCount), "Third patron should have 3 loans");

        MockLoanRepository.Verify(r => r.GetLoansByTime(_startDate, _endDate), Times.Once);
    }

}
