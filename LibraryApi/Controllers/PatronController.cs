using BusinessLogic.InventoryInsights;
using LibraryApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronController : ControllerBase
{
    private readonly UserActivity _userActivity;
    private readonly ILogger<PatronController> _logger;

    public PatronController(UserActivity userActivity, ILogger<PatronController> logger)
    {
        _userActivity = userActivity ?? throw new ArgumentNullException(nameof(userActivity));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
