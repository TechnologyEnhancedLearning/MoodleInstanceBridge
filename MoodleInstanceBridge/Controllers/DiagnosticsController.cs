using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Interfaces;

namespace MoodleInstanceBridge.Controllers
{
    /// <summary>
    /// Diagnostics endpoint for configuration status
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IInstanceConfigurationService _configurationService;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(
            IInstanceConfigurationService configurationService,
            ILogger<DiagnosticsController> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        /// <summary>
        /// Get configuration status and validation errors
        /// </summary>
        /// <returns>Configuration diagnostics information</returns>
        [HttpGet("configuration")]
        public async Task<IActionResult> GetConfigurationStatus()
        {
            try
            {
                var configurations = await _configurationService.GetAllConfigurationsAsync();
                var validationResults = _configurationService.LastValidationResults;
                var lastRefresh = _configurationService.LastRefreshTime;

                var response = new
                {
                    lastRefreshTime = lastRefresh?.ToString("o"),
                    totalConfigurations = validationResults.Count,
                    validConfigurations = validationResults.Count(r => r.IsValid),
                    invalidConfigurations = validationResults.Count(r => !r.IsValid),
                    activeConfigurations = configurations.Count,
                    configurations = configurations.Select(c => new
                    {
                        shortName = c.ShortName,
                        baseUrl = c.BaseUrl,
                        weighting = c.Weighting,
                        enabledEndpoints = c.EnabledEndpoints,
                        isEnabled = c.IsEnabled
                    }).ToList(),
                    validationErrors = validationResults
                        .Where(r => !r.IsValid)
                        .Select(r => new
                        {
                            instanceShortName = r.InstanceShortName,
                            errors = r.Errors
                        })
                        .ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve configuration diagnostics");
                return StatusCode(500, new
                {
                    error = "Failed to retrieve configuration diagnostics",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Trigger a manual configuration refresh
        /// </summary>
        /// <returns>Refresh results</returns>
        [HttpPost("configuration/refresh")]
        public async Task<IActionResult> RefreshConfiguration()
        {
            try
            {
                _logger.LogInformation("Manual configuration refresh triggered");
                
                var results = await _configurationService.RefreshConfigurationsAsync();

                var response = new
                {
                    refreshTime = DateTime.UtcNow.ToString("o"),
                    totalConfigurations = results.Count,
                    validConfigurations = results.Count(r => r.IsValid),
                    invalidConfigurations = results.Count(r => !r.IsValid),
                    validationResults = results.Select(r => new
                    {
                        instanceShortName = r.InstanceShortName,
                        isValid = r.IsValid,
                        errors = r.Errors
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh configuration");
                return StatusCode(500, new
                {
                    error = "Failed to refresh configuration",
                    detail = ex.Message
                });
            }
        }
    }
}
