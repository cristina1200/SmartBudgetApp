using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SmartBudget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetSummary(int userId)
        {
            var summary = await _dashboardService.GetSummaryAsync(userId);

            return Ok(summary);
        }
    }
}