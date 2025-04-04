using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using PlexVideoConverter.Hubs;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

/// <summary>
/// This is the SignalR Client for the backend
/// </summary>
public class PvcConversionClient
{
    private static readonly Lazy<PvcConversionClient> _instance = new (() => new PvcConversionClient());
    public static PvcConversionClient Instance => _instance.Value;
    
    private static IHubContext<PvcConversionHub> _hubContext;
    
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Initialize(IHubContext<PvcConversionHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task QueueStatus(QueueStatusArgs status)
    {
        await _hubContext.Clients.All.SendAsync("QueueStatus", status);
    }

    public async Task SendConversionProgressUpdate(Guid conversionProcessId, int progress)
    {
        await _hubContext.Clients.All.SendAsync("ConversionProgressUpdate", conversionProcessId, progress);
    }
}

public class QueueStatusArgs
{
    public List<ConversionProcess> QueuedProcesses { get; set; }
    public List<ConversionProcess> ActiveProcesses { get; set; }
    public List<ConversionProcess> CompletedProcesses { get; set; }
}