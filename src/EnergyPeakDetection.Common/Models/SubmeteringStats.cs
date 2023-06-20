namespace EnergyPeakDetection.Common;

public class SubmeteringStats
{
    public string Key { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public double Value { get; set; }

    public override string ToString()
    {
        return $"{ Key }:{ Date }:{ Time }:{ Value }"; 
    }
}
