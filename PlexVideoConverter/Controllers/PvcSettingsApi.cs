using Microsoft.AspNetCore.Mvc;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

namespace PlexVideoConverter.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PvcSettingsApi: ControllerBase
{
    private readonly ILogger<PvcSettingsApi> _logger;

    public PvcSettingsApi(ILogger<PvcSettingsApi> logger)
    {
        _logger = logger;
        _logger.LogInformation("Starting Pvc Settings Api");
    }
    
    [HttpGet("/ffmpeg/settings")]
    public ActionResult<FfmpegSettings> GetSystemSettings()
    {
        try
        {
            _logger.LogInformation("GET /ffmpeg/settings");
            var fs = SettingsService.Instance.FfmpegSettings;
            return Ok(fs);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in /ffmpeg/settings", ex);
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("/ffmpeg/settings")]
    public ActionResult GetSystemSettings([FromBody] FfmpegSettings fs)
    {
        try
        {
            _logger.LogInformation("POST /ffmpeg/settings");
            SettingsService.Instance.FfmpegSettings = fs;
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in /ffmpeg/settings", ex);
            return BadRequest(ex.Message);
        }
    }
}