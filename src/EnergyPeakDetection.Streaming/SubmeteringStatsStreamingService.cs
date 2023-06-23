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
    private KafkaStream _stream;

    public SubmeteringStatsStreamingService(ILogger<SubmeteringStatsStreamingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _windowDuration = configuration.GetValue<int>("PeakDetectionWindowDuration");
        var statsTopic = configuration["Kafka:SubmeteringStatsTopicName"];
        var peaksTopic = configuration["Kafka:PeaksTopicName"];

        var streamConfig = new StreamConfig<StringSerDes, StatsSerdes>();
        streamConfig.ApplicationId = "peak-detection";
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
        .Peek((k, v) => _logger.LogDebug($"Key: '{ k }', sum: '{ v.Sum }', count: '{ v.Count }.'"))
        .To(outputTopic, new TimeWindowedSerDes<string>(new StringSerDes(), 1000), new SubmeteringAggregateStatsSerdes());

        return streamBuilder.Build();
    }


}