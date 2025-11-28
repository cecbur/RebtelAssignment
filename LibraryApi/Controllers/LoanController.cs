using Microsoft.AspNetCore.Mvc;
using DataStorageContracts;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<LoanController> _logger;

    public LoanController(ILoanRepository loanRepository, ILogger<LoanController> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
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
}
