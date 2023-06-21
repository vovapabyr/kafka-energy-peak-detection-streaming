using System.Text.Json;
using Confluent.Kafka;
using EnergyPeakDetection.Common;
using Streamiz.Kafka.Net.SerDes;

namespace EnergyPeakDetection.Streaming;
public class StatsSerdes : AbstractSerDes<SubmeteringStats>
{
    public override SubmeteringStats Deserialize(byte[] data, SerializationContext context)
    {
        return JsonSerializer.Deserialize<SubmeteringStats>(data);
    }

    public override byte[] Serialize(SubmeteringStats data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}