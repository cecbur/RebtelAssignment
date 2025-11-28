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
    /// Get the average reading pace (pages per day) for a specific lone
    /// </summary>
    /// <param name="loneId">The ID of the lone</param>
    /// <returns>The patron's reading pace in pages per day. Null if the book is not yet returned</returns>
    [HttpGet("{loneId}/reading-pace-pages-per-day")]
    public async Task<ActionResult<LoneReadingPaceResponse>> GetReadingPacePagesPerDay(int loneId)
    {
        try
        {
            var pagesPerDay = await _userActivity.GetPagesPerDay(loneId);

            if (pagesPerDay == null)
            {
                return Ok(new LoneReadingPaceResponse
                {
                    LoneId = loneId,
                    PagesPerDay = null,
                    Message = "This loan has not been returned"
                });
            }

            return Ok(new LoneReadingPaceResponse
            {
                LoneId = loneId,
                PagesPerDay = pagesPerDay.Value,
                Message = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reading pace for lone ID {loneId}", loneId);
            return StatusCode(500, "An error occurred while calculating reading pace");
        }
    }
}
