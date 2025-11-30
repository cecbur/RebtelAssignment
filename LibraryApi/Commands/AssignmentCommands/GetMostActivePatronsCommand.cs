using BusinessLogicContracts.Interfaces;
using LibraryApi.DTOs;
using Grpc.Core;

namespace LibraryApi.Commands.AssignmentCommands;

/// <summary>
/// For XML comment, see the calling controller
/// </summary>
public class GetMostActivePatronsCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<GetMostActivePatronsCommand> logger)
{
    private readonly IBusinessLogicFacade _businessLogicFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
    private readonly ILogger<GetMostActivePatronsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(bool success, PatronLoanFrequencyResponse[] response)> GetMostActivePatrons(
        DateTime startDate,
        DateTime endDate,
        int maxPatrons)
    {
        if (!IsValid(startDate, endDate, maxPatrons))
            return (false, []);

        try
        {
            _logger.LogInformation("Getting most active patrons from {StartDate} to {EndDate} (max: {MaxPatrons})",
                startDate, endDate, maxPatrons);

            var patronLoans = await _businessLogicFacade.GetPatronsOrderedByLoanFrequency(startDate, endDate);

            if (patronLoans.Count() > maxPatrons)
                patronLoans = patronLoans.Take(maxPatrons).ToArray();

            var response = patronLoans.Select(pl => new PatronLoanFrequencyResponse
            {
                PatronId = pl.Patron.Id,
                PatronName = $"{pl.Patron.FirstName} {pl.Patron.LastName}",
                LoanCount = pl.LoanCount
            }).ToArray();

            _logger.LogInformation("Retrieved {Count} patrons with loan activity", response.Length);

            return (true, response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting most active patrons from {StartDate} to {EndDate}",
                startDate, endDate);
            return (false, []);
        }
        catch (Exception ex) when (ex is not ArgumentNullException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error getting most active patrons from {StartDate} to {EndDate}",
                startDate, endDate);
            return (false, []);
        }
    }

    private bool IsValid(DateTime startDate, DateTime endDate, int maxPatrons)
    {
        if (startDate == default)
        {
            _logger.LogWarning("Invalid startDate: {StartDate}. Cannot be default value.", startDate);
            return false;
        }

        if (endDate == default)
        {
            _logger.LogWarning("Invalid endDate: {EndDate}. Cannot be default value.", endDate);
            return false;
        }

        if (startDate >= endDate)
        {
            _logger.LogWarning("Invalid date range: startDate {StartDate} must be before endDate {EndDate}.",
                startDate, endDate);
            return false;
        }

        if (maxPatrons <= 0)
        {
            _logger.LogWarning("Invalid maxPatrons value: {MaxPatrons}. Must be positive (greater than 0).", maxPatrons);
            return false;
        }

        return true;
    }
}
