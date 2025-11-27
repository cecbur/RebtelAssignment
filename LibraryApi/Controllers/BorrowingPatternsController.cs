using Microsoft.AspNetCore.Mvc;
using BusinessLogic;
using DataStorage.Repositories;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

/// <summary>
/// API Controller for analyzing borrowing patterns
/// Demonstrates SOLID principles with dependency injection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BorrowingPatternsController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BorrowingPatternsController> _logger;

    public BorrowingPatternsController(
        IBookService bookService,
        IBookRepository bookRepository,
        ILogger<BorrowingPatternsController> logger)
    {
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes borrowing patterns based on book IDs
    /// Returns books with power-of-two IDs and odd-numbered IDs
    /// </summary>
    /// <returns>Analysis of borrowing patterns</returns>
    /// <response code="200">Returns the analysis results</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("analyze")]
    [ProducesResponseType(typeof(BorrowingPatternsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BorrowingPatternsResponse>> AnalyzePatternsAsync()
    {
        try
        {
            _logger.LogInformation("Starting borrowing patterns analysis");

            // Get all book IDs from database
            var bookIds = (await _bookRepository.GetAllBooks()).Select(b => b.BookId).ToList();

            if (!bookIds.Any())
            {
                _logger.LogWarning("No books found in database");
                return Ok(new BorrowingPatternsResponse { TotalBooksAnalyzed = 0 });
            }

            // Find book IDs that are powers of two
            var powerOfTwoIds = bookIds
                .Where(id => _bookService.IsPowerOfTwo(id))
                .ToList();

            // Find book IDs that are odd numbers
            var oddIds = _bookService.GetOddNumberedBookIds(bookIds).ToList();

            var response = new BorrowingPatternsResponse
            {
                PowerOfTwoBookIds = powerOfTwoIds,
                OddNumberedBookIds = oddIds,
                TotalBooksAnalyzed = bookIds.Count
            };

            _logger.LogInformation(
                "Borrowing patterns analysis completed. Total books: {TotalBooks}, Power of Two: {PowerOfTwo}, Odd: {Odd}",
                bookIds.Count, powerOfTwoIds.Count, oddIds.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing borrowing patterns");
            return StatusCode(500, "An error occurred while analyzing borrowing patterns");
        }
    }

    /// <summary>
    /// Checks if a specific book ID is a power of two
    /// </summary>
    /// <param name="bookId">The book ID to check</param>
    /// <returns>Boolean indicating if the ID is a power of two</returns>
    /// <response code="200">Returns the result</response>
    /// <response code="400">If the book ID is invalid</response>
    [HttpGet("is-power-of-two/{bookId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<bool> IsPowerOfTwo(int bookId)
    {
        try
        {
            if (bookId <= 0)
            {
                return BadRequest("Book ID must be a positive integer");
            }

            var result = _bookService.IsPowerOfTwo(bookId);
            _logger.LogInformation("Book ID {BookId} power of two check: {Result}", bookId, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if book ID {BookId} is power of two", bookId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all odd-numbered book IDs from the database
    /// </summary>
    /// <returns>List of odd-numbered book IDs</returns>
    /// <response code="200">Returns the list of odd book IDs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("odd-book-ids")]
    [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<int>>> GetOddBookIdsAsync()
    {
        try
        {
            var books = await _bookRepository.GetAllBooks();
            var oddIds = _bookService.GetOddNumberedBookIds(books.Select(b => b.BookId)).ToList();

            _logger.LogInformation("Retrieved {Count} odd-numbered book IDs", oddIds.Count());
            return Ok(oddIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving odd-numbered book IDs");
            return StatusCode(500, "An error occurred while retrieving odd-numbered book IDs");
        }
    }
}
