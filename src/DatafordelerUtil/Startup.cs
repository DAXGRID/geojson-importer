using Microsoft.Extensions.Logging;

namespace DatafordelerUtil;

internal class Startup
{
    private readonly ILogger<Startup> _logger;
    private readonly Settings _settings;

    public Startup(ILogger<Startup> logger, Settings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting {ServiceName}", nameof(Startup));
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
