using NLog;
using PlexVideoConverter.Models;
using PlexVideoConverter.Models.FileBrowser;

namespace PlexVideoConverter.Services;

public class ConversionQueueService
{
    private static readonly Lazy<ConversionQueueService> _instance = new (() => new ConversionQueueService());
    public static ConversionQueueService Instance => _instance.Value;
    
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private SemaphoreSlim sem;
    
    public Dictionary<Guid, ConversionProcess> QueuedProcesses { get; set;  } = new();
    public Dictionary<Guid, ConversionProcess> ActiveProcesses { get; set; } = new();
    public Dictionary<Guid, ConversionProcess> CompletedProcesses { get; set; } = new();

    public ConversionQueueService()
    {
        sem = new SemaphoreSlim(1, 1);
    }
    
    public async void AddItems(ConversionProcess fp)
    {
        QueuedProcesses.Add(fp.Id, fp);
        await Instance.Enqueue(() => FfmpegCoreService.Instance.ConvertVideoAsync(ref fp), fp.Id);
        
        ActiveProcesses.Remove(fp.Id);
        CompletedProcesses.Add(fp.Id, fp);
        FfmpegCoreService.Instance.CompleteFileConversion(fp);
    }

    private async Task Enqueue(Func<Task> taskGenerator, Guid processId)
    {
        await sem.WaitAsync();
        try
        {
            logger.Info("Tasked started...");

            MoveProcessToActive(processId);
            PvcConversionClient.Instance.QueueStatus(GetQueueStatus());
            if (QueuedProcesses.ContainsKey(processId))
                await taskGenerator();
            else logger.Warn($"Process {processId} not found, skipping");
        }
        finally
        {
            logger.Info("Task finished processing...");
            sem.Release();
        }
    }

    public void RemoveItem(List<FileNode> nodesToDequeue)
    {
        nodesToDequeue.ForEach(node =>
        {
            var process = QueuedProcesses
                .FirstOrDefault(qp => qp.Value.InputName == node.Name).Value;
            if (process != null)
                Dequeue(process.Id);
        });
    }

    private void Dequeue(Guid processId)
    {
        QueuedProcesses.Remove(processId);
    }

    private void MoveProcessToActive(Guid processId)
    {
        var process = QueuedProcesses[processId];
        QueuedProcesses.Remove(processId);
        ActiveProcesses.Add(processId, process);
    }

    public QueueStatusArgs GetQueueStatus()
    {
        return new QueueStatusArgs
        {
            QueuedProcesses = Instance.QueuedProcesses
                .Select(dict => dict.Value).ToList(),
            ActiveProcesses = Instance.ActiveProcesses
                .Select(dict => dict.Value).ToList(),
            CompletedProcesses = Instance.CompletedProcesses
                .Select(dict => dict.Value).ToList(),
        };
    }
}