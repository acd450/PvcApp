using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

namespace PlexVideoConverter.Controllers;

[Route("api/conversions")]
[ApiController]
public sealed class ConversionHistoryApi(ConversionHistoryService history) : ControllerBase
{
    /// <summary>Gets lifetime conversion totals and the history snapshot ID.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(LifetimeConversionStatsResponse), StatusCodes.Status200OK)]
    public ActionResult<LifetimeConversionStatsResponse> GetStats(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(history.GetStats());
    }

    /// <summary>Gets a bounded, newest-first history page within a lifetime snapshot.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ConversionHistoryPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ConversionHistoryPageResponse> GetHistory(
        CancellationToken cancellationToken,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 25,
        [FromQuery, Required, Range(typeof(long), "0", "9223372036854775807")] long? snapshotId = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(history.GetHistory(page, pageSize, snapshotId!.Value));
    }
}