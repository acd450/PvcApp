using Microsoft.AspNetCore.Mvc;
using PlexVideoConverter.Models.FileBrowser;
using PlexVideoConverter.Services.FileBrowser;

namespace PlexVideoConverter.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileBrowserApi: ControllerBase
{
    private readonly ILogger<FileBrowserApi> _logger;

    public FileBrowserApi(ILogger<FileBrowserApi> logger)
    {
        _logger = logger;
        _logger.LogInformation("Starting File Browser Api");
    }
    
    [HttpPost("/file/children")]
    public ActionResult<List<FileNode>> GetChildren([FromBody] FileChildrenRequest req)
    {
        _logger.LogInformation($"POST: [{Request.Path}] - Body.Path=[{req.Path}]");
        try
        {
            return FileBrowserService.GetDirectoryChildrenNodes(req);
        }
        catch (Exception ex)
        {
            var errorMessage = $"ERROR during [POST:{Request.Path}] - Body.Path=[{req.Path}]: {ex.Message}";
            _logger.LogError(errorMessage, ex);
            return BadRequest(errorMessage);
        }
    }
    
    [HttpGet("/file/root")]
    public ActionResult<List<DriveNode>> GetRootFileBrowser()
    {
        _logger.LogInformation($"POST: [{Request.Path}]");
        try
        {
            return FileBrowserService.GetRootDrives();
        }
        catch (Exception ex)
        {
            var errorMessage = $"ERROR during [POST:{Request.Path}]: {ex.Message}";
            _logger.LogError(errorMessage, ex);
            return BadRequest(errorMessage);
        }
    }
}