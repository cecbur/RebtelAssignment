using BusinessLogicContracts.Interfaces;
using LibraryApi.Converters;
using LibraryApi.DTOs;
using Grpc.Core;

namespace LibraryApi.Commands.AssignmentCommands;

/// <summary>
/// For XML comment, see the calling controller
/// </summary>
public class GetBooksSortedByMostLoanedCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<GetBooksSortedByMostLoanedCommand> logger)
{
    private readonly IBusinessLogicFacade _businessLogicGrpcFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
    private readonly ILogger<GetBooksSortedByMostLoanedCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(bool success, BookLoansResponse[] response)> GetBooksSortedByMostLoaned(int? maxBooks)
    {
        if (!IsValid(maxBooks))
            return (false, []);

        try
        {
            _logger.LogInformation("Getting most loaned books sorted by loan count (max: {MaxBooks})", maxBooks?.ToString() ?? "unlimited");

            var bookLoans = await _businessLogicGrpcFacade.GetBooksSortedByMostLoaned(maxBooks);
            var response = BookLoansResponseConverter.ToDto(bookLoans);

            _logger.LogInformation("Retrieved {Count} books sorted by loan count", response.Length);

            return (true, response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting most loaned books");
            return (false, []);
        }
        catch (Exception ex) when (ex is not ArgumentNullException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error getting most loaned books");
            return (false, []);
        }
    }

    private bool IsValid(int? maxBooks)
    {
        if (maxBooks <= 0)
        {
            _logger.LogWarning($"Invalid maxBooks value: {maxBooks}. Must be positive.");
            return false;
        }

        return true;
    }
}
