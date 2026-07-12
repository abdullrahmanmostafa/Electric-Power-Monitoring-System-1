using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.Services;
using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class AdminTierController : ControllerBase
    {
        private readonly ITierService _tierService;

        public AdminTierController(ITierService tierService)
        {
            _tierService = tierService;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            // يمكن إضافة مفتاح إداري هنا (X-Admin-Key)
            var settings = await _tierService.GetTierSettingsAsync();
            return Ok(settings);
        }

        [HttpPost("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] List<TierSettingsDto> settings)
        {
            await _tierService.UpdateTierSettingsAsync(settings);
            return Ok(new { message = "Tier settings updated" });
        }
    }
}