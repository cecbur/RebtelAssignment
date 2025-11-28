using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using DataStorageContracts;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanRepository _loanRepository;
    private readonly UserActivity _userActivity;
    private readonly ILogger<LoanController> _logger;

    public LoanController(ILoanRepository loanRepository, UserActivity userActivity, ILogger<LoanController> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _userActivity = userActivity ?? throw new ArgumentNullException(nameof(userActivity));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    
    

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessModels.Loan>>> GetAllLoans()
    {
        try
        {
            var loans = await _loanRepository.GetAllLoans();
            return Ok(loans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all loans via gRPC");
            return StatusCode(500, "An error occurred while retrieving loans");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BusinessModels.Loan>> GetLoanById(int id)
    {
        try
        {
            var loan = await _loanRepository.GetLoanById(id);
            return Ok(loan);
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
    public async Task<ActionResult<IEnumerable<BusinessModels.Loan>>> GetActiveLoans()
    {
        try
        {
            var loans = await _loanRepository.GetActiveLoans();
            return Ok(loans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active loans via gRPC");
            return StatusCode(500, "An error occurred while retrieving active loans");
        }
    }

    [HttpGet("by-time")]
    public async Task<ActionResult<IEnumerable<BusinessModels.Loan>>> GetLoansByTime([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var loans = await _loanRepository.GetLoansByTime(startDate, endDate);
            return Ok(loans);
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
            var pagesPerDay = await _userActivity.GetPagesPerDay(loanId);

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
