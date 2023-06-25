# Energy Peak Detection With Kafka Streaming 
## About
The main objective of this project is to detect energy consumption peaks with Kafka Streams. To emulate the stream of data coming from submetering devices (where each corresponds to a separate room), we use the [dataset](https://github.com/rekharchandran/Analysis-of-Energy-Power-Consumption-IoT-Analytics/blob/master/Data%20Set/household_power_consumption.zip). The Kafka Streams client is written in C# with the help of [Streamiz package](https://lgouellec.github.io/kafka-streams-dotnet/) ([github](https://lgouellec.github.io/kafka-streams-dotnet/))

### Partitions and partitioner
As we have three submetering devices, we would create a 'submetering-stats-topic' topic with three partitions, where each partition would get data from a single specific submetering device. To reach this we would use a custom partitioner which would distribute stats in the following manner:
 - stats with 'Sub_metering_1' key would go to the first partition
 - stats with 'Sub_metering_2' key would go to the second partition
 - stats with 'Sub_metering_3' key would go to the third partition

This would allow us to process stats coming from each submetering device in parallel.

### Topology
To detect outliers we would use the simple IQR (Inter-Quartile Range) Method of Outlier Detection [link](https://towardsdatascience.com/why-1-5-in-iqr-method-of-outlier-detection-5d07fdc82097). According to the article, the upper bound for outliers is equal to ```Q3 + 1.5 * (Q3 - Q1)```, and if rely on Gaussian Distribution it would be equal to ```mean + 2.7 * sigma```, where ```sigma``` is the standard deviation and which could be configured in [appSettings.json](https://github.com/vovapabyr/kafka-energy-peak-detection-streaming/blob/main/src/EnergyPeakDetection.Streaming/appsettings.json) of the streaming project. The more value you set to the ```StdDeviation``` the more extreme peaks it would only detect. So, we know how we can detect energy peaks, now we need to decide how often we would like to apply IQR method for an upcoming stream of data. The submetering devices in our dataset send their stats each minute. So, ex. we can collect upcoming stats for 10 minutes, and then apply IQR method to detect if there were any anomalies. To do that we would use kafka streams windows. In order not to miss any peak, we should use a sliding window, but because Streamiz doesn't support sliding windows, we could also use a hopping window with a step size of one minute (as we are getting new stat each minute). The window size ```PeakDetectionWindowSize``` can also be configurable in [appSettings.json](https://github.com/vovapabyr/kafka-energy-peak-detection-streaming/blob/main/src/EnergyPeakDetection.Streaming/appsettings.json):
```
{
  "Kafka": {
    "BootstrapServers": "kafka-1:9092,kafka-2:9092,kafka-3:9092",
    "SubmeteringStatsTopicName": "submetering-stats-topic",
    "PeaksTopicName": "peaks-topic"
  },
  "PeakDetectionWindowSize": 10,
  "StdDeviation": 15
}
```
So, to summarize we have data coming from the 'submetering-stats-topic' topic, we aggregate data in windows, apply IQR method to detect peaks within each window, and then put detected peaks to the 'peaks-topic' topic. Because we have our windows overlapping, we do deduplication on the consumer side which reads from 'peaks-topic' topic. To check the topology see ```BuildEnergyPeakDetectionTopology``` method of the [SubmeteringStatsStreamingService](https://github.com/vovapabyr/kafka-energy-peak-detection-streaming/blob/main/src/EnergyPeakDetection.Streaming/SubmeteringStatsStreamingService.cs) class.
**Note. The output stream which represents aggregation would contain all changes of the underlying KTable, which means in our case we would need to use ```suppress``` method of kafka streams to get the last state of window aggregation. But, because Streamiz library doesn't support the ```suppress``` method we get our final aggregation in stream, with the help of filtering out all intermediate states whose stats count is less than window size.**  




 
