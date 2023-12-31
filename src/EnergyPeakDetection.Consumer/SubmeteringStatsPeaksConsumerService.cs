using Confluent.Kafka;
using EnergyPeakDetection.Common;

namespace EnergyPeakDetection.Consumer;
public class SubmeteringStatsPeaksConsumerService : BackgroundService
{
    private readonly ILogger<SubmeteringStatsPeaksConsumerService> _logger;
    private readonly SubmeteringPeaksInMemoryStore _store;
    private readonly SubmeteringPeaksHubService _peaksHubService;
    private readonly string _peaksTopic;
    private IConsumer<string, SubmeteringStats> _kafkaConsumer;

    public SubmeteringStatsPeaksConsumerService(ILogger<SubmeteringStatsPeaksConsumerService> logger, SubmeteringPeaksInMemoryStore store, SubmeteringPeaksHubService peaksHubService, IConfiguration configuration)
    {
        _logger = logger;
        _store = store;
        _peaksHubService = peaksHubService;
        _peaksTopic = configuration["Kafka:PeaksTopicName"];
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "peaks-consumers",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _kafkaConsumer = new ConsumerBuilder<string, SubmeteringStats>(consumerConfig).SetValueDeserializer(new SubmeteringStatsDeserializer()).Build();
    }

    public override void Dispose()
    {
        _kafkaConsumer.Close();
        _kafkaConsumer.Dispose();

        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(_peaksTopic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var cr = _kafkaConsumer.Consume(cancellationToken);
                var peak = cr.Message.Value;
                if(_store.TryAdd(peak))
                {
                    _logger.LogInformation($"PEAK: { peak }");
                    await _peaksHubService.PushNewDetectedPeakAsync(peak);   
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                _logger.LogWarning($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected error: {e}");
                break;
            }
        }
    }
}