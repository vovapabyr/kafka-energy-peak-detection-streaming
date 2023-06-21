namespace EnergyPeakDetection.Producer;

public class SubmeteringsStatsCsvRecord
{
    public const string DateColumnKey = "Date";
    public const string TimeColumnKey = "Time";
    public const string Submetering1ColumnKey = "Sub_metering_1";
    public const string Submetering2ColumnKey = "Sub_metering_2";
    public const string Submetering3ColumnKey = "Sub_metering_3";

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public double Submetering1 { get; set; }

    public double Submetering2 { get; set; }

    public double Submetering3 { get; set; }

    public IEnumerable<KeyValuePair<string, double>> GetSubmeteringsStats()
    {
        yield return new KeyValuePair<string, double>(Submetering1ColumnKey, Submetering1);
        yield return new KeyValuePair<string, double>(Submetering2ColumnKey, Submetering2);
        yield return new KeyValuePair<string, double>(Submetering3ColumnKey, Submetering3);
    }
}