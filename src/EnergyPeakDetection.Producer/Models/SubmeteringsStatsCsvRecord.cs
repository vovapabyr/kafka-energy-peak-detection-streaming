namespace EnergyPeakDetection.Producer;

public class SubmeteringsStatsCsvRecord
{
    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public double Submetering1 { get; set; }

    public double Submetering2 { get; set; }

    public double Submetering3 { get; set; }
}