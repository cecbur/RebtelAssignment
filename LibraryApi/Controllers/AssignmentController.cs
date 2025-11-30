using BusinessLogicContracts.Interfaces;
using LibraryApi.Commands;
using LibraryApi.Commands.AssignmentCommands;
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
    private readonly GetBooksSortedByMostLoanedCommand _booksSortedByMostLoanedCommand;
    private readonly GetMostActivePatronsCommand _mostActivePatronsCommand;
    private readonly GetReadingPacePagesPerDayCommand _readingPacePagesPerDayCommand;
    private readonly GetOtherBooksBorrowedCommand _otherBooksBorrowedCommand;

    public AssignmentController(
        GetBooksSortedByMostLoanedCommand booksSortedByMostLoanedCommand,
        GetMostActivePatronsCommand mostActivePatronsCommand,
        GetReadingPacePagesPerDayCommand readingPacePagesPerDayCommand,
        GetOtherBooksBorrowedCommand otherBooksBorrowedCommand)
    {
        _booksSortedByMostLoanedCommand = booksSortedByMostLoanedCommand ?? throw new ArgumentNullException(nameof(booksSortedByMostLoanedCommand));
        _mostActivePatronsCommand = mostActivePatronsCommand ?? throw new ArgumentNullException(nameof(mostActivePatronsCommand));
        _readingPacePagesPerDayCommand = readingPacePagesPerDayCommand ?? throw new ArgumentNullException(nameof(readingPacePagesPerDayCommand));
        _otherBooksBorrowedCommand = otherBooksBorrowedCommand ?? throw new ArgumentNullException(nameof(otherBooksBorrowedCommand));
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
        var (success,  response) = await _booksSortedByMostLoanedCommand.GetBooksSortedByMostLoaned(maxBooks);
        if (success)
            return Ok(response);
        return StatusCode(500, "An error occurred while retrieving book loan statistics");
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
        var (success, response) = await _mostActivePatronsCommand.GetMostActivePatrons(startDate, endDate, maxPatrons);
        if (success)
            return Ok(response);
        return StatusCode(500, "An error occurred while retrieving most active patrons");
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
        var (success, response) = await _readingPacePagesPerDayCommand.GetReadingPacePagesPerDay(loanId);
        if (success)
            return Ok(response);
        return StatusCode(500, "An error occurred while calculating reading pace");
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
        var (success, response) = await _otherBooksBorrowedCommand.GetOtherBooksBorrowed(bookId);
        if (success)
            return Ok(response);
        return StatusCode(500, "An error occurred while retrieving associated books");
    }

}
