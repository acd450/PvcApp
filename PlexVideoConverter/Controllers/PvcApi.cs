using Microsoft.AspNetCore.Mvc;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

namespace PlexVideoConverter.Controllers;

[Route("pvc/[controller]")]
public class PvcApi: Controller
{
    private readonly ILogger<PvcApi> _logger;

    public PvcApi(ILogger<PvcApi> logger)
    {
        _logger = logger;
        _logger.LogInformation("Starting PVC API");
    }
    
    [HttpGet("/files/queued")]
    public ActionResult<List<FileProcess>> GetFilesInQueue()
    {
        try
        {
            _logger.LogInformation("GET /files/queued");
            var fps = FfmpegCoreService.Instance.FileProcesses.Select(fp => fp.Value)
                .ToList();
            return Ok(fps);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in /files/queued", ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("/files/settings")]
    public ActionResult<List<FileListenerSettings>> GetFilesInSettings()
    {
        try
        {
            _logger.LogInformation("GET /files/settings");
            var fls = SettingsService.Instance.FileListenerSettings;
            return Ok(fls);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in /files/queued", ex);
            return BadRequest(ex.Message);
        }
    }
}