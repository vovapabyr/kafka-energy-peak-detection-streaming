using System.Text.Json;
using Confluent.Kafka;
using EnergyPeakDetection.Common;
using Streamiz.Kafka.Net.SerDes;

namespace EnergyPeakDetection.Streaming;

public class SubmeteringAggregateStatsSerdes : AbstractSerDes<SubmeteringAggregateStats>
{
    public override SubmeteringAggregateStats Deserialize(byte[] data, SerializationContext context)
    {
        return JsonSerializer.Deserialize<SubmeteringAggregateStats>(data);
    }

    public override byte[] Serialize(SubmeteringAggregateStats data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}