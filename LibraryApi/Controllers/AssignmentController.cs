using BusinessLogicContracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Converters;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

/// <summary>
/// API Controller for assignment-related endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssignmentController : ControllerBase
{
    private readonly IBusinessLogicFacade _businessLogicGrpcFacade;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(
        IBusinessLogicFacade businessLogicFacade,
        ILogger<AssignmentController> logger)
    {
        _businessLogicGrpcFacade = businessLogicFacade ?? throw new ArgumentNullException(nameof(businessLogicFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 1. Inventory Insights: What are the most borrowed books? 
    /// Gets all books sorted by how many times they were loaned (most loaned first)
    /// </summary>
    /// <param name="maxBooks">Optional maximum number of books to return</param>
    /// <returns>List of books with their loan counts, ordered by loan count descending</returns>
    /// <response code="200">Returns the list of books sorted by loan count</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("most-loaned-books")]
    [ProducesResponseType(typeof(IEnumerable<BookLoansResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BookLoansResponse>>> GetBooksSortedByMostLoaned([FromQuery] int? maxBooks = null)
    {
        try
        {
            _logger.LogInformation("Getting most loaned books sorted by loan count (max: {MaxBooks})", maxBooks ?? -1);

            var bookLoans = await _businessLogicGrpcFacade.GetBooksSortedByMostLoaned(maxBooks);
            var response = BookLoansResponseConverter.ToDto(bookLoans);

            _logger.LogInformation("Retrieved {Count} books sorted by loan count", response.Length);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most loaned books");
            return StatusCode(500, "An error occurred while retrieving book loan statistics");
        }
    }

    /// <summary>
    /// 2. User Activity: Which users have borrowed the most books within a given time frame?
    /// Gets patrons ordered by loan frequency within a given time frame
    /// </summary>
    /// <param name="startDate">Start date of the time frame</param>
    /// <param name="endDate">End date of the time frame</param>
    /// <param name="maxPatrons">The maximum number of patrons to return</param>
    /// <returns>List of patrons ordered by loan count (descending)</returns>
    /// <response code="200">List of patrons ordered by loan count (descending)</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("most-active-patrons")]
    [ProducesResponseType(typeof(IEnumerable<PatronLoanFrequencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PatronLoanFrequencyResponse>>> GetMostActivePatrons(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int maxPatrons)
    {
        try
        {
            _logger.LogInformation("Getting most active patrons from {StartDate} to {EndDate}", startDate, endDate);

            var patronLoans = await _businessLogicGrpcFacade.GetPatronsOrderedByLoanFrequency(startDate, endDate);

            if (patronLoans.Count() > maxPatrons)
                patronLoans = patronLoans.Take(maxPatrons).ToArray();

            var response = patronLoans.Select(pl => new PatronLoanFrequencyResponse
            {
                PatronId = pl.Patron.Id,
                PatronName = $"{pl.Patron.FirstName} {pl.Patron.LastName}",
                LoanCount = pl.LoanCount
            }).ToList();

            _logger.LogInformation("Retrieved {Count} patrons with loan activity", response.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most active patrons from {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving most active patrons");
        }
    }

    /// <summary>
    /// 3. User Activity: Estimate a users reading pace (pages per day)
    ///    based on the borrow and return duration of a book, assuming continuous reading.
    /// Get the average reading pace (pages per day) for a specific loan
    /// </summary>
    /// <param name="loanId">The ID of the loan</param>
    /// <returns>The patron's reading pace in pages per day. Null if the book is not yet returned</returns>
    /// <response code="200">Returns the reading pace information</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("reading-pace-pages-per-day/{loanId}")]
    [ProducesResponseType(typeof(LoanReadingPaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoanReadingPaceResponse>> GetReadingPacePagesPerDay(int loanId)
    {
        try
        {
            var pagesPerDay = await _businessLogicGrpcFacade.GetPagesPerDay(loanId);

            if (pagesPerDay == null)
            {
                return Ok(new LoanReadingPaceResponse
                {
                    LoanId = loanId,
                    PagesPerDay = null,
                    Message = "This loan has not been returned"
                });
            }

            return Ok(new LoanReadingPaceResponse
            {
                LoanId = loanId,
                PagesPerDay = pagesPerDay.Value,
                Message = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reading pace for loan ID {LoanId}", loanId);
            return StatusCode(500, "An error occurred while calculating reading pace");
        }
    }
    
    /// <summary>
    /// 4. Borrowing patterns: What other books were borrowed by individuals who borrowed a specific book?
    /// Gets other books that were borrowed by individuals who borrowed a specific book,
    /// filtered to books borrowed more than once and ordered by frequency by loan ratios.
    /// I.e. how much more often was the other book borrowed compared to the specific book
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

            var bookFrequencies = await _businessLogicGrpcFacade.GetOtherBooksBorrowed(bookId);
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
