using CinemaReservation.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticService _analyticService;

        public AnalyticsController(IAnalyticService analyticService)
        {
            _analyticService = analyticService;
        }

        [HttpGet("capacity")]
        public async Task<IActionResult> GetShowtimeCapacities([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var capacities = await _analyticService.GetShowtimeCapacitiedAsync(fromDate, toDate);
            return Ok(capacities);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetMoviedRevenues([FromQuery] DateTime? fromdate, [FromQuery] DateTime? toDate)
        {
            var revenues = await _analyticService.GetMovieRevenuesAsync(fromdate, toDate);
            return Ok(revenues);
        }

        [HttpGet("top-customers/{count}")]
        public async Task<IActionResult> GetTopCustomersAsync([FromRoute] int count)
        {
            var topCustomers = await _analyticService.GetTopcustomersbyCountAsync(count);
            return Ok(topCustomers);
        }

        [HttpGet("cancelations-info")]
        public async Task<IActionResult> GetMostCanceledMovieAndLostRevenueAsync()
        {
            var result = await _analyticService.GetMostCanceledMovieAndLostRevenueServiceAsync();
            return Ok(result);
        }
    }
}
