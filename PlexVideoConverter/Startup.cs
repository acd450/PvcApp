using System.Diagnostics;
using PlexVideoConverter.Services;

namespace PlexVideoConverter;

public class Startup : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public Startup(
        IHostApplicationLifetime hostApplicationLifetime)
        => _hostApplicationLifetime = hostApplicationLifetime;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private void OnStopping()
    {
        SettingsService.Instance.npmProcess?.Kill();
    }
}