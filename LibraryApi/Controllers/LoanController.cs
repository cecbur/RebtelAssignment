using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using DataStorageContracts;
using LibraryApi.DTOs;
using LibraryApi.Converters;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanRepository _loanRepository;
    private readonly PatronActivity _patronActivity;
    private readonly ILogger<LoanController> _logger;

    public LoanController(ILoanRepository loanRepository, PatronActivity patronActivity, ILogger<LoanController> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _patronActivity = patronActivity ?? throw new ArgumentNullException(nameof(patronActivity));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    
    

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans()
    {
        try
        {
            var loans = await _loanRepository.GetAllLoans();
            var loanDtos = LoanDtoConverter.ToDto(loans);
            return Ok(loanDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all loans via gRPC");
            return StatusCode(500, "An error occurred while retrieving loans");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetLoanById(int id)
    {
        try
        {
            var loan = await _loanRepository.GetLoanById(id);
            var loanDto = LoanDtoConverter.ToDto(loan);
            return Ok(loanDto);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Loan with id {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loan {LoanId} via gRPC", id);
            return StatusCode(500, "An error occurred while retrieving the loan");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetActiveLoans()
    {
        try
        {
            var loans = await _loanRepository.GetActiveLoans();
            var loanDtos = LoanDtoConverter.ToDto(loans);
            return Ok(loanDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active loans via gRPC");
            return StatusCode(500, "An error occurred while retrieving active loans");
        }
    }

    [HttpGet("by-time")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoansByTime([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var loans = await _loanRepository.GetLoansByTime(startDate, endDate);
            var loanDtos = LoanDtoConverter.ToDto(loans);
            return Ok(loanDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loans by time range {StartDate} to {EndDate} via gRPC", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving loans by time");
        }
    }

    /// <summary>
    /// Get the average reading pace (pages per day) for a specific loan
    /// </summary>
    /// <param name="loanId">The ID of the loan</param>
    /// <returns>The patron's reading pace in pages per day. Null if the book is not yet returned</returns>
    [HttpGet("{loanId}/reading-pace-pages-per-day")]
    public async Task<ActionResult<LoanReadingPaceResponse>> GetReadingPacePagesPerDay(int loanId)
    {
        try
        {
            var pagesPerDay = await _patronActivity.GetPagesPerDay(loanId);

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
            _logger.LogError(ex, $"Error getting reading pace for loan ID {loanId}", loanId);
            return StatusCode(500, "An error occurred while calculating reading pace");
        }
    }
}
