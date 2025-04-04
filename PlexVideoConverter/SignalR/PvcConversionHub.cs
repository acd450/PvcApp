using Microsoft.AspNetCore.SignalR;
using PlexVideoConverter.Models;
using PlexVideoConverter.Models.FileBrowser;
using PlexVideoConverter.Services;

namespace PlexVideoConverter.Hubs;

/// <summary>
/// This is the SignalR Hub that clients will hit
/// </summary>
public class PvcConversionHub : Hub
{
    public async Task QueueConversion(List<FileNode> nodesToQueue)
    {
        //await Clients.All.SendAsync("UpdateQueue", msg);
        nodesToQueue.ForEach(ntq =>
        {
            ConversionQueueService.Instance.AddItems(new ConversionProcess(ntq));
        });
    }
    public async Task DequeueConversion(List<FileNode> nodesToDequeue)
    {
        //await Clients.All.SendAsync("UpdateQueue", msg);
        ConversionQueueService.Instance.RemoveItem(nodesToDequeue);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
        
        //Sends the new attached client the current queue status
        await Clients.Client(Context.ConnectionId).SendAsync("QueueStatus", ConversionQueueService.Instance.GetQueueStatus());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class QueueMessageArgs
{
    public List<FileNode> Enqueue { get; set; }
    public List<FileNode> Dequeue { get; set; }
}