using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoodleInstanceBridge.Controllers
{
    [AllowAnonymous]
    [ApiController]

    // API Version 2
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/health")]
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
