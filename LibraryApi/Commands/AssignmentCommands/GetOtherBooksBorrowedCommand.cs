using BusinessLogicContracts.Interfaces;
using LibraryApi.Converters;
using LibraryApi.DTOs;
using Grpc.Core;

namespace LibraryApi.Commands.AssignmentCommands;

/// <summary>
/// For XML comment, see the calling controller
/// </summary>
public class GetOtherBooksBorrowedCommand(
    IBusinessLogicFacade businessLogicFacade,
    ILogger<GetOtherBooksBorrowedCommand> logger)
{
    private readonly IBusinessLogicFacade _businessLogicFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
    private readonly ILogger<GetOtherBooksBorrowedCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(bool success, BookFrequencyResponse[] response)> GetOtherBooksBorrowed(int bookId)
    {
        if (!IsValid(bookId))
            return (false, []);

        try
        {
            _logger.LogInformation("Getting other books borrowed for book id {BookId}", bookId);

            var bookFrequencies = await _businessLogicFacade.GetOtherBooksBorrowed(bookId);
            var response = BookFrequencyResponseConverter.ToDto(bookFrequencies);

            _logger.LogInformation("Retrieved {Count} associated books for book id {BookId}",
                response.Length, bookId);

            return (true, response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting other books borrowed for book id {BookId}", bookId);
            return (false, []);
        }
        catch (Exception ex) when (ex is not ArgumentNullException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error getting other books borrowed for book id {BookId}", bookId);
            return (false, []);
        }
    }

    private bool IsValid(int bookId)
    {
        if (bookId <= 0)
        {
            _logger.LogWarning("Invalid bookId value: {BookId}. Must be positive (greater than 0).", bookId);
            return false;
        }

        return true;
    }
}
