using Confluent.Kafka;
using EnergyPeakDetection.Common;
using Streamiz.Kafka.Net.Crosscutting;
using Streamiz.Kafka.Net.Processors;

namespace EnergyPeakDetection.Streaming;

public class SubmeteringStatsEventTimeExtractor : ITimestampExtractor
{
    private ILogger<SubmeteringStatsStreamingService> _logger;

    public SubmeteringStatsEventTimeExtractor(ILogger<SubmeteringStatsStreamingService> logger)
    {
        _logger = logger;
    }

    public long Extract(ConsumeResult<object, object> record, long partitionTime)
    {
        var stats = record.Message.Value as SubmeteringStats;
        if (stats == null)
            return -1;

        var time = stats.Date;
        time = time.Add(stats.Time);
        
        var timeSec = time.GetMilliseconds();
        _logger.LogDebug($"Event date: '{ time }', milliseconds: '{ timeSec }'.");
        return timeSec;
    }
}