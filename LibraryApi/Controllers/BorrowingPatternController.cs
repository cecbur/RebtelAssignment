using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Converters;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BorrowingPatternController : ControllerBase
{
    private readonly BorrowingPatterns _borrowingPatterns;
    private readonly ILogger<BorrowingPatternController> _logger;

    public BorrowingPatternController(
        BorrowingPatterns borrowingPatterns,
        ILogger<BorrowingPatternController> logger)
    {
        _borrowingPatterns = borrowingPatterns ?? throw new ArgumentNullException(nameof(borrowingPatterns));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets other books that were borrowed by individuals who borrowed a specific book,
    /// filtered to books borrowed more than once and ordered by frequency with loan ratios
    /// </summary>
    /// <param name="bookId">The ID of the book to analyze</param>
    /// <returns>Associated books with frequency ratios, ordered by frequency descending</returns>
    [HttpGet("other-books-borrowed/{bookId}")]
    [ProducesResponseType(typeof(BookFrequencyResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookFrequencyResponse[]>> GetOtherBooksBorrowed(int bookId)
    {
        try
        {
            _logger.LogInformation("Getting other books borrowed for book id {BookId}", bookId);

            var bookFrequencies = await _borrowingPatterns.GetPatronLoansOrderedByFrequency(bookId);
            var response = BookFrequencyResponseConverter.ToDto(bookFrequencies);

            _logger.LogInformation("Retrieved {Count} associated books for book id {BookId}",
                response.Length, bookId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting other books borrowed for book id {BookId}", bookId);
            return StatusCode(500, "An error occurred while retrieving associated books");
        }
    }
}
