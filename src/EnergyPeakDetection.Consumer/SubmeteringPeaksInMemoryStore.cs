using EnergyPeakDetection.Common;

namespace EnergyPeakDetection.Consumer;

public class SubmeteringPeaksInMemoryStore
{
    private readonly Dictionary<string, SubmeteringStats> _peaks = new Dictionary<string, SubmeteringStats>();
    private ILogger<SubmeteringPeaksInMemoryStore> _logger;

    public SubmeteringPeaksInMemoryStore(ILogger<SubmeteringPeaksInMemoryStore> logger)
    {
        _logger = logger;
    }

    public bool TryAdd(SubmeteringStats peak)
    {
        return _peaks.TryAdd(peak.GetKey(), peak);
    }
}