using EnergyPeakDetection.Common;
using Microsoft.AspNetCore.SignalR;

namespace EnergyPeakDetection.Consumer;

public class SubmeteringPeaksHubService
{
    private readonly IHubContext<SubmeteringPeaksHub> _peaksHub;

    private readonly ILogger<SubmeteringPeaksHubService> _logger;

    public SubmeteringPeaksHubService(IHubContext<SubmeteringPeaksHub> hubContext, ILogger<SubmeteringPeaksHubService> logger)
    {
        _peaksHub = hubContext;
        _logger = logger;
    }

    public async Task PushNewDetectedPeakAsync(SubmeteringStats peak)
    {
        await _peaksHub.Clients.All.SendAsync("newPeak", new { Room = peak.Key, Date = peak.GetDateTime(), Value = peak.Value });
    }
}