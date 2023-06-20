using System.Text.Json;
using Confluent.Kafka;

namespace EnergyPeakDetection.Common;
public class SubmeteringStatsDeserializer : IDeserializer<SubmeteringStats>
{
    SubmeteringStats IDeserializer<SubmeteringStats>.Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<SubmeteringStats>(data);
    }
}