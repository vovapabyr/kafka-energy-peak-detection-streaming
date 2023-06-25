using EnergyPeakDetection.Common;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace EnergyPeakDetection.Streaming;

public class SubmeteringStatsStreamingService : BackgroundService
{
    private readonly ILogger<SubmeteringStatsStreamingService> _logger;
    private readonly int _windowDuration;
    private readonly double _stdDeviation;
    private KafkaStream _stream;

    public SubmeteringStatsStreamingService(ILogger<SubmeteringStatsStreamingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _windowDuration = configuration.GetValue<int>("PeakDetectionWindowSize");
        _stdDeviation = configuration.GetValue<double>("StdDeviation");
        var statsTopic = configuration["Kafka:SubmeteringStatsTopicName"];
        var peaksTopic = configuration["Kafka:PeaksTopicName"];

        var streamConfig = new StreamConfig<StringSerDes, StatsSerdes>();
        streamConfig.ApplicationId = "peaks-detectors";
        streamConfig.BootstrapServers = configuration["Kafka:BootstrapServers"];
        streamConfig.DefaultTimestampExtractor = new SubmeteringStatsEventTimeExtractor(_logger);
        var streamTopology = BuildEnergyPeakDetectionTopology(statsTopic, peaksTopic);
        _stream = new KafkaStream(streamTopology, streamConfig);
    }

    public override void Dispose()
    {
        _stream.Dispose();
        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return StartStatsProcessingAsync(stoppingToken);
    }

    private async Task StartStatsProcessingAsync(CancellationToken cancellationToken)
    {
        await _stream.StartAsync(cancellationToken);

        _logger.LogInformation("Processing finished");
    }

    private Topology BuildEnergyPeakDetectionTopology(string inputTopic, string outputTopic)
    {
        var streamBuilder = new StreamBuilder();
        streamBuilder.Stream<string, SubmeteringStats>(inputTopic)
        .Peek((k, v) => _logger.LogDebug($"Processing '{ v }'."))
        .GroupByKey()
        .WindowedBy(HoppingWindowOptions.Of(TimeSpan.FromMinutes(_windowDuration), TimeSpan.FromMinutes(1)))
        .Aggregate(() => new SubmeteringAggregateStats(), (k, v, s) => 
        {
            s.Sum += v.Value;
            s.Count++;
            s.Stats.Add(v);
            return s;
        }, InMemoryWindows.As<string, SubmeteringAggregateStats>().WithValueSerdes<SubmeteringAggregateStatsSerdes>())
        .ToStream()
        .Filter((k, v) => v.Count == _windowDuration)
        .MapValues((k, v) => 
        {
            var mean = v.Sum / v.Count;
            // Standard deviation.
            v.StdDeviation = Math.Sqrt(v.Stats.Sum(v => Math.Pow(v.Value - mean, 2)) / v.Count);
            // Upper bound outliers = Q3 + 1.5 * (Q3 - Q1) = mean + 2.7 * stdDev
            var upperBound = mean + 2.7 * _stdDeviation;
            foreach (var stat in v.Stats)
            {
                if (stat.Value > upperBound)
                    v.Peaks.Add(stat); 
            }

            _logger.LogDebug($"Key: '{ k }'. Outliers upper bound: '{ upperBound }'. Number of peaks found: '{ v.Peaks.Count }'.");
            return v;
        })
        .FlatMap((k, v) => v.Peaks.Select(e => new KeyValuePair<string, SubmeteringStats>(k.Key, e)))
        .Peek((k, v) => _logger.LogInformation($"PEAK: '{ v }'."))
        .To(outputTopic);

        return streamBuilder.Build();
    }
}