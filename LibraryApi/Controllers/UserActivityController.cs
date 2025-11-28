using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserActivityController : ControllerBase
{
    private readonly UserActivity _userActivity;
    private readonly ILogger<UserActivityController> _logger;

    public UserActivityController(
        UserActivity userActivity,
        ILogger<UserActivityController> logger)
    {
        _userActivity = userActivity ?? throw new ArgumentNullException(nameof(userActivity));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets patrons ordered by loan frequency within a given time frame
    /// Answers the question: "Which users have borrowed the most books within a given time frame?"
    /// </summary>
    /// <param name="startDate">Start date of the time frame</param>
    /// <param name="endDate">End date of the time frame</param>
    /// <param name="maxPatrons">The maximum number of patrons to return</param>
    /// <returns>List of patrons ordered by loan count (descending)</returns>
    [HttpGet("most-active-patrons")]
    [ProducesResponseType(typeof(IEnumerable<PatronLoanFrequencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PatronLoanFrequencyResponse>>> GetMostActivePatrons(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int maxPatrons)
    {
        try
        {
            _logger.LogInformation("Getting most active patrons from {StartDate} to {EndDate}", startDate, endDate);

            var patronLoans = await _userActivity.GetPatronLoansOrderedByFrequency(startDate, endDate);

            if (patronLoans.Count() > maxPatrons)
                patronLoans = patronLoans.Take(maxPatrons).ToArray();

            var response = patronLoans.Select(pl => new PatronLoanFrequencyResponse
            {
                PatronId = pl.Patron.Id,
                PatronName = $"{pl.Patron.FirstName} {pl.Patron.LastName}",
                LoanCount = pl.LoanCount
            }).ToList();

            _logger.LogInformation("Retrieved {Count} patrons with loan activity", response.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most active patrons from {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving most active patrons");
        }
    }
}
