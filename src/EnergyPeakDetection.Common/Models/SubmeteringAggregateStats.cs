namespace EnergyPeakDetection.Common;

public class SubmeteringAggregateStats
{
    public SubmeteringAggregateStats()
    {
        Stats = new List<SubmeteringStats>();
    }

    public double Sum { get; set; }

    public int Count { get; set; }

    public List<SubmeteringStats> Stats { get; set; }
}