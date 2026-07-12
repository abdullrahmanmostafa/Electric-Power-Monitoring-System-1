using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.Services;
using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TierController : ControllerBase
    {
        private readonly ITierService _tierService;

        public TierController(ITierService tierService)
        {
            _tierService = tierService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            var status = await _tierService.GetUserTierStatusAsync(userIdentifier);
            return Ok(status);
        }
    }
}