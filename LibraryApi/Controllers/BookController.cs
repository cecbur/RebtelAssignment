using Microsoft.AspNetCore.Mvc;
using BusinessLogic;
using DataStorage.Repositories;
using DataStorage.Entities;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

/// <summary>
/// API Controller for book operations
/// Handles CRUD operations and title manipulations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BookController> _logger;

    public BookController(
        IBookService bookService,
        IBookRepository bookRepository,
        ILogger<BookController> logger)
    {
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all books from the library
    /// </summary>
    /// <returns>List of all books</returns>
    /// <response code="200">Returns the list of books</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        var bookDtos = books.Select(MapToDto);

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
        var book = await _bookRepository.GetBookByIdAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book with ID {BookId} not found", id);
            return NotFound($"Book with ID {id} not found");
        }

        return Ok(MapToDto(book));
    }

    /// <summary>
    /// Reverses a book title
    /// </summary>
    /// <param name="request">The title operation request</param>
    /// <returns>The reversed title</returns>
    /// <response code="200">Returns the reversed title</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("reverse-title")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<string> ReverseTitle([FromBody] TitleOperationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var reversedTitle = _bookService.ReverseTitle(request.Title);
            _logger.LogInformation("Reversed title: '{Original}' -> '{Reversed}'", request.Title, reversedTitle);
            return Ok(reversedTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing title");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Generates replicas of a book title
    /// </summary>
    /// <param name="request">The title replica request</param>
    /// <returns>The replicated title</returns>
    /// <response code="200">Returns the replicated title</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("generate-replicas")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<string> GenerateTitleReplicas([FromBody] TitleReplicaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var replicatedTitle = _bookService.GenerateTitleReplicas(request.Title, request.Count);
            _logger.LogInformation("Generated {Count} replicas of title: '{Title}'", request.Count, request.Title);
            return Ok(replicatedTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating title replicas");
            return BadRequest(ex.Message);
        }
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
        var books = await _bookRepository.SearchBooksByTitleAsync(titlePattern);
        var bookDtos = books.Select(MapToDto);

        _logger.LogInformation("Found {Count} books matching pattern '{Pattern}'", bookDtos.Count(), titlePattern);
        return Ok(bookDtos);
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
            var book = MapToEntity(bookDto);
            var createdBook = await _bookRepository.AddBookAsync(book);

            _logger.LogInformation("Created new book with ID {BookId}: '{Title}'", createdBook.BookId, createdBook.Title);
            return CreatedAtAction(nameof(GetBookByIdAsync), new { id = createdBook.BookId }, MapToDto(createdBook));
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
        if (id != bookDto.BookId)
        {
            return BadRequest("ID in URL does not match ID in body");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var book = MapToEntity(bookDto);
            var success = await _bookRepository.UpdateBookAsync(book);

            if (!success)
            {
                _logger.LogWarning("Book with ID {BookId} not found for update", id);
                return NotFound($"Book with ID {id} not found");
            }

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
            var success = await _bookRepository.DeleteBookAsync(id);

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

    // Helper methods for mapping between Entity and DTO
    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            IsAvailable = book.IsAvailable
        };
    }

    private static Book MapToEntity(BookDto dto)
    {
        return new Book
        {
            BookId = dto.BookId,
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            PublicationYear = dto.PublicationYear,
            IsAvailable = dto.IsAvailable
        };
    }
}
