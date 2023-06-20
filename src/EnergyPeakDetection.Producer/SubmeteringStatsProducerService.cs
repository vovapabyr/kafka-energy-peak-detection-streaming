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
                        _kafkaProducer.Produce(_topic, new Message<string, SubmeteringStats> {  Key = KafkaConstants.Submetering1Key, Value = new SubmeteringStats() { Key = KafkaConstants.Submetering1Key, Date = item.Date, Time = item.Time, Value = item.Submetering1 } });
                        _kafkaProducer.Produce(_topic, new Message<string, SubmeteringStats> {  Key = KafkaConstants.Submetering2Key, Value = new SubmeteringStats() { Key = KafkaConstants.Submetering2Key, Date = item.Date, Time = item.Time, Value = item.Submetering2 } });
                        _kafkaProducer.Produce(_topic, new Message<string, SubmeteringStats> {  Key = KafkaConstants.Submetering3Key, Value = new SubmeteringStats() { Key = KafkaConstants.Submetering3Key, Date = item.Date, Time = item.Time, Value = item.Submetering3 } });
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