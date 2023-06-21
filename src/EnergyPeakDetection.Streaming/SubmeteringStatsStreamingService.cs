
using System.Text.Json;
using Confluent.Kafka;
using EnergyPeakDetection.Common;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace EnergyPeakDetection.Streaming;

public class SubmeteringStatsStreamingService : BackgroundService
{
    private readonly ILogger<SubmeteringStatsStreamingService> _logger;
    private KafkaStream _stream;

    public SubmeteringStatsStreamingService(ILogger<SubmeteringStatsStreamingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var statsTopic = configuration["Kafka:SubmeteringStatsTopicName"];
        var peaksTopic = configuration["Kafka:PeaksTopicName"];

        var streamConfig = new StreamConfig<StringSerDes, StatsSerdes>();
        streamConfig.ApplicationId = "peak-detection";
        streamConfig.BootstrapServers = configuration["Kafka:BootstrapServers"];

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
        streamBuilder.Stream<string, SubmeteringStats>(inputTopic).Peek((k, v) => _logger.LogDebug($"Processing '{ v }'.")).To(outputTopic);

        return streamBuilder.Build();
    }
}