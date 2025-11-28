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
    /// Get the average reading pace (pages per day) for a specific patron
    /// </summary>
    /// <param name="patronId">The ID of the patron</param>
    /// <returns>The patron's reading pace in pages per day</returns>
    [HttpGet("{patronId}/reading-pace-pages-per-day")]
    public async Task<ActionResult<PatronReadingPaceResponse>> GetReadingPacePagesPerDay(int patronId)
    {
        try
        {
            var pagesPerDay = await _userActivity.GetPagesPerDayByPatron(patronId);

            if (pagesPerDay == null)
            {
                return Ok(new PatronReadingPaceResponse
                {
                    PatronId = patronId,
                    PagesPerDay = null,
                    Message = "No completed loans with page count information found for this patron"
                });
            }

            return Ok(new PatronReadingPaceResponse
            {
                PatronId = patronId,
                PagesPerDay = pagesPerDay.Value,
                Message = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reading pace for patron {PatronId}", patronId);
            return StatusCode(500, "An error occurred while calculating reading pace");
        }
    }
}
