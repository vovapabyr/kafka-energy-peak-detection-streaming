using System.Text.Json;
using Confluent.Kafka;

namespace EnergyPeakDetection.Common;
public class SubmeteringStatsSerializer : ISerializer<SubmeteringStats>
{
    public byte[] Serialize(SubmeteringStats data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}