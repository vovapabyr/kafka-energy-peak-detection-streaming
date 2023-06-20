namespace EnergyPeakDetection.Producer;
public sealed class CsvSubmeteringStatsRecordMap : CsvHelper.Configuration.ClassMap<SubmeteringsStatsCsvRecord>
{
    public CsvSubmeteringStatsRecordMap()
    {
        Map(m => m.Date).TypeConverterOption.Format("d/M/yyyy").Name("Date");
        Map(m => m.Time).TypeConverterOption.Format("c").Name("Time");
        Map(m => m.Submetering1).Name("Sub_metering_1");
        Map(m => m.Submetering2).Name("Sub_metering_2");
        Map(m => m.Submetering3).Name("Sub_metering_3");
    }
}