using System.Globalization;
using Confluent.Kafka;
using CsvHelper;
using CsvHelper.Configuration;
using EnergyPeakDetection.Common;

namespace EnergyPeakDetection.Producer;
public class SubmeteringStatsProducerService : BackgroundService
{
    private readonly ILogger<SubmeteringStatsProducerService> _logger;
    private readonly string _topic;
    private readonly string _datasetName;
    private IProducer<string, SubmeteringStats> _kafkaProducer;

    public SubmeteringStatsProducerService(ILogger<SubmeteringStatsProducerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _topic = configuration["Kafka:SubmeteringStatsTopicName"];
        _datasetName = configuration["DatasetName"];
        var config = new ProducerConfig()
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        _kafkaProducer = new ProducerBuilder<string, SubmeteringStats>(config).SetValueSerializer(new SubmeteringStatsSerializer()).Build();
    }

    public override void Dispose()
    {
        _kafkaProducer.Dispose();

        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return StartReadingDatasetAsync(stoppingToken); 
    }

    private async Task StartReadingDatasetAsync(CancellationToken cancellationToken)
    {
        using (var reader = new StreamReader($"dataset//{ _datasetName }"))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
        {
            csv.Context.RegisterClassMap<CsvSubmeteringStatsRecordMap>();
            _logger.LogInformation("Started processing of dataset.");
            var totalStatsCount = 0;
            var failedStatsCount = 0;
            try
            {
                while(await csv.ReadAsync())
                {
                    try
                    {
                        var item = csv.GetRecord<SubmeteringsStatsCsvRecord>();
                        _logger.LogDebug($"New submetering stat: '{ item.Date }', '{ item.Time }', '{ item.Submetering1 }', '{ item.Submetering2 }', '{ item.Submetering3 }'");
                        int i = 0;
                        foreach (var stats in item.GetSubmeteringsStats())
                            _kafkaProducer.Produce(new TopicPartition(_topic, new Partition(i++)), new Message<string, SubmeteringStats> { Key = stats.Key, Value = new SubmeteringStats() { Key = stats.Key, Date = item.Date, Time = item.Time, Value = stats.Value } });
                    }
                    catch (Exception ex)
                    {
                        failedStatsCount++;
                        _logger.LogError(ex, string.Empty);
                    }
                    finally { totalStatsCount++; } 
                }
            }
            catch (ProduceException<string, SubmeteringStats> ex)
            {
                _logger.LogWarning($"Failed to process submetering stats: '{ ex.DeliveryResult.Value }'.");
            }

            _kafkaProducer.Flush(cancellationToken);
            _logger.LogInformation($"Finished processing stats. Total count: '{ totalStatsCount }'. Failed count: '{ failedStatsCount }'.");
        }
    }
}
