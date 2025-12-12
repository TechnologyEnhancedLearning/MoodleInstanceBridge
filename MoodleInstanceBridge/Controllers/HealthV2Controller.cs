using Microsoft.AspNetCore.Mvc;

namespace MoodleInstanceBridge.Controllers
{
    [ApiController]

    // API Version 2
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/healthtestv2")]
    public class HealthV2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new
            {
                status = "Healthy",
                version = "2.0",
                serverTimeUtc = DateTime.UtcNow
            });
    }
}
