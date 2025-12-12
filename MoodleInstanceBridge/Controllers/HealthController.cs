using Microsoft.AspNetCore.Mvc;

namespace MoodleInstanceBridge.Controllers
{
    [ApiController]

    // API Version 1
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/healthTestV1")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { status = "Healthy", version = "1.0" });
    }
}
