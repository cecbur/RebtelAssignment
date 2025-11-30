using BusinessLogicContracts.Interfaces;
using LibraryApi.DTOs;
using Grpc.Core;

namespace LibraryApi.Commands.AssignmentCommands;

/// <summary>
/// For XML comment, see the calling controller
/// </summary>
public class GetReadingPacePagesPerDayCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<GetReadingPacePagesPerDayCommand> logger)
{
    private readonly IBusinessLogicFacade _businessLogicFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
    private readonly ILogger<GetReadingPacePagesPerDayCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(bool success, LoanReadingPaceResponse response)> GetReadingPacePagesPerDay(int loanId)
    {
        if (!IsValid(loanId))
            return (false, new LoanReadingPaceResponse { LoanId = loanId });

        try
        {
            _logger.LogInformation("Getting reading pace for loan ID {LoanId}", loanId);

            var pagesPerDay = await _businessLogicFacade.GetPagesPerDay(loanId);

            if (pagesPerDay == null)
            {
                _logger.LogInformation("Loan ID {LoanId} has not been returned yet", loanId);
                return (true, new LoanReadingPaceResponse
                {
                    LoanId = loanId,
                    PagesPerDay = null,
                    Message = "This loan has not been returned or the number of pages is unknown"
                });
            }

            _logger.LogInformation("Retrieved reading pace for loan ID {LoanId}: {PagesPerDay} pages/day",
                loanId, pagesPerDay.Value);

            return (true, new LoanReadingPaceResponse
            {
                LoanId = loanId,
                PagesPerDay = pagesPerDay.Value,
                Message = null
            });
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting reading pace for loan ID {LoanId}", loanId);
            return (false, new LoanReadingPaceResponse { LoanId = loanId });
        }
        catch (Exception ex) when (ex is not ArgumentNullException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error getting reading pace for loan ID {LoanId}", loanId);
            return (false, new LoanReadingPaceResponse { LoanId = loanId });
        }
    }

    private bool IsValid(int loanId)
    {
        if (loanId <= 0)
        {
            _logger.LogWarning("Invalid loanId value: {LoanId}. Must be positive (greater than 0).", loanId);
            return false;
        }

        return true;
    }
}
