using BusinessLogic;
using LibraryApi.Converters;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

/// <summary>
/// Controller for analyzing book borrowing patterns and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookPatternsController(
    BookPatterns bookPatterns,
    ILogger<BookPatternsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all books sorted by how many times they were borrowed (most borrowed first)
    /// </summary>
    /// <returns>List of books with their loan counts, ordered by loan count descending</returns>
    /// <response code="200">Returns the list of books sorted by loan count</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("most-borrowed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DTOs.BookLoansResponse>>> GetMostBorrowedBooksSorted()
    {
        try
        {
            logger.LogInformation("Getting most borrowed books sorted by loan count");

            var bookLoans = await bookPatterns.GetMostBorrowedBooksSorted();
            var response = BookLoansResponseConverter.ToDto(bookLoans);

            logger.LogInformation("Retrieved {Count} books sorted by loan count", response.Length);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting most borrowed books");
            return StatusCode(500, "An error occurred while retrieving book statistics");
        }
    }
}
