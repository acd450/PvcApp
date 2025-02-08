using Microsoft.AspNetCore.Mvc;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

namespace PlexVideoConverter.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FolderStatsApi: ControllerBase
{
    private readonly ILogger<FolderStatsApi> _logger;

    public FolderStatsApi(ILogger<FolderStatsApi> logger)
    {
        _logger = logger;
        _logger.LogInformation("Starting Folder Stats Api");
    }

    [HttpPost("/folder/workingdir")]
    public ActionResult SetWorkingDirectory([FromBody] string workingDirectory)
    {
        try
        {
            FolderStatsService.Instance.WorkingDirectory = workingDirectory;
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/folder/stats")]
    public ActionResult<FolderStats> GetStats()
    {
        try
        {
            return FolderStatsService.Instance.GetWorkingDirectoryStats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ex.Message);
        }
    }
    
}