using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompareController : ControllerBase
    {
        private readonly IReadingRepository _readingRepo;

        public CompareController(IReadingRepository readingRepo)
        {
            _readingRepo = readingRepo;
        }

        [HttpGet]
        public async Task<IActionResult> ComparePeriods(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime period1Start,
            [FromQuery] DateTime period1End,
            [FromQuery] DateTime period2Start,
            [FromQuery] DateTime period2End)
        {
            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            var consumption1 = await _readingRepo.GetConsumptionBetweenAsync(hubSerial, plugNumber, period1Start, period1End);
            var consumption2 = await _readingRepo.GetConsumptionBetweenAsync(hubSerial, plugNumber, period2Start, period2End);

            decimal percentChange = 0;
            if (consumption2 != 0)
                percentChange = ((consumption1 - consumption2) / consumption2) * 100;
            else if (consumption1 != 0)
                percentChange = 100;

            var increase = consumption1 > consumption2;

            return Ok(new CompareResponseDto
            {
                ConsumptionPeriod1Wh = consumption1,
                ConsumptionPeriod2Wh = consumption2,
                PercentChange = Math.Round(percentChange, 2),
                Increase = increase
            });
        }
    }
}