using BusinessLogicContracts.Interfaces;
using LibraryApi.Controllers;
using LibraryApi.Converters;
using LibraryApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Commands.AssignmentCommands;

/// For XML comment, se the calling controller
public class GetBooksSortedByMostLoanedCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<AssignmentController> logger)
{
    private readonly IBusinessLogicFacade _businessLogicGrpcFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
    private readonly ILogger<AssignmentController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(bool success, BookLoansResponse[] response)> TryGetBooksSortedByMostLoaned(int? maxBooks)
    {
        try
        {
            _logger.LogInformation("Getting most loaned books sorted by loan count (max: {MaxBooks})", maxBooks ?? -1);

            var bookLoans = await _businessLogicGrpcFacade.GetBooksSortedByMostLoaned(maxBooks);
            var response = BookLoansResponseConverter.ToDto(bookLoans);

            _logger.LogInformation("Retrieved {Count} books sorted by loan count", response.Length);

            return (true, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most loaned books");
            return (false, []);
        }
    }


}
