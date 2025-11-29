using Microsoft.AspNetCore.Mvc;
using BusinessLogic;
using DataStorageContracts;
using BusinessModels;
using DataStorage.Exceptions;
using LibraryApi.Converters;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

/// <summary>
/// API Controller for book operations
/// Handles CRUD operations and title manipulations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookController(
    IBookRepository bookRepository,
    BookPatterns bookPatterns,
    ILogger<BookController> logger)
    : ControllerBase
{
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
    private readonly BookPatterns _bookPatterns = bookPatterns ?? throw new ArgumentNullException(nameof(bookPatterns));
    private readonly ILogger<BookController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets all books from the library
    /// </summary>
    /// <returns>List of all books</returns>
    /// <response code="200">Returns the list of books</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllBooks();
        var bookDtos = books.Select(BookDtoConverter.ToDto);

        _logger.LogInformation("Retrieved {Count} books", bookDtos.Count());
        return Ok(bookDtos);
    }

    /// <summary>
    /// Gets a specific book by ID
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <returns>The book details</returns>
    /// <response code="200">Returns the book</response>
    /// <response code="404">If the book is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetBookByIdAsync(int id)
    {
        Book book;
        try
        {
            book = await _bookRepository.GetBookById(id);
        }
        catch (BookIdMissingException e)
        {
            _logger.LogWarning(e.Message, e.BookId);
            return NotFound($"Book with ID {id} not found");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e.Message, e);
            return NotFound($"Internal error");
        }

        return Ok(BookDtoConverter.ToDto(book));
    }


    /// <summary>
    /// Searches books by title pattern
    /// </summary>
    /// <param name="titlePattern">The search pattern</param>
    /// <returns>List of matching books</returns>
    /// <response code="200">Returns the matching books</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooksByTitleAsync([FromQuery] string titlePattern)
    {
        var books = await _bookRepository.SearchBooksByTitleLikeQuery(titlePattern);
        var bookDtos = books.Select(BookDtoConverter.ToDto);

        _logger.LogInformation("Found {Count} books matching pattern '{Pattern}'", bookDtos.Count(), titlePattern);
        return Ok(bookDtos);
    }

    /// <summary>
    /// Gets all books sorted by how many times they were loaned (most loaned first)
    /// </summary>
    /// <param name="maxBooks">Optional maximum number of books to return</param>
    /// <returns>List of books with their loan counts, ordered by loan count descending</returns>
    /// <response code="200">Returns the list of books sorted by loan count</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("most-loaned")]
    [ProducesResponseType(typeof(IEnumerable<BookLoansResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BookLoansResponse>>> GetMostLoanedBooksSorted([FromQuery] int? maxBooks = null)
    {
        try
        {
            _logger.LogInformation("Getting most loaned books sorted by loan count (max: {MaxBooks})", maxBooks ?? -1);

            var bookLoans = await _bookPatterns.GetMostLoanedBooksSorted(maxBooks);
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
    /// Adds a new book to the library
    /// </summary>
    /// <param name="bookDto">The book details</param>
    /// <returns>The created book</returns>
    /// <response code="201">Returns the created book</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDto>> CreateBookAsync([FromBody] BookDto bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var book = BookDtoConverter.FromDto(bookDto);
            var createdBook = await _bookRepository.AddBook(book);

            _logger.LogInformation("Created new book with ID {BookId}: '{Title}'", createdBook.Id, createdBook.Title);
            return CreatedAtAction(nameof(GetBookByIdAsync), new { id = createdBook.Id }, BookDtoConverter.ToDto(createdBook));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing book
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <param name="bookDto">The updated book details</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the update was successful</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the book is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBookAsync(int id, [FromBody] BookDto bookDto)
    {
        if (id != bookDto.Id)
        {
            return BadRequest("ID in URL does not match ID in body");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var book = BookDtoConverter.FromDto(bookDto);
            Book updatedBook = await _bookRepository.UpdateBook(book);
            _logger.LogInformation("Updated book with ID {BookId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with ID {BookId}", id);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a book from the library
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the deletion was successful</response>
    /// <response code="404">If the book is not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBookAsync(int id)
    {
        try
        {
            var success = await _bookRepository.DeleteBook(id);

            if (!success)
            {
                _logger.LogWarning("Book with ID {BookId} not found for deletion", id);
                return NotFound($"Book with ID {id} not found");
            }

            _logger.LogInformation("Deleted book with ID {BookId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book with ID {BookId}", id);
            return StatusCode(500, "An error occurred while deleting the book");
        }
    }
}
