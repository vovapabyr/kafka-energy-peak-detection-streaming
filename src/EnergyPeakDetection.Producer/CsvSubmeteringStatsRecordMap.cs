namespace EnergyPeakDetection.Producer;
public sealed class CsvSubmeteringStatsRecordMap : CsvHelper.Configuration.ClassMap<SubmeteringsStatsCsvRecord>
{
    public CsvSubmeteringStatsRecordMap()
    {
        Map(m => m.Date).TypeConverterOption.Format("d/M/yyyy").Name(SubmeteringsStatsCsvRecord.DateColumnKey);
        Map(m => m.Time).TypeConverterOption.Format("c").Name(SubmeteringsStatsCsvRecord.TimeColumnKey);
        Map(m => m.Submetering1).Name(SubmeteringsStatsCsvRecord.Submetering1ColumnKey);
        Map(m => m.Submetering2).Name(SubmeteringsStatsCsvRecord.Submetering2ColumnKey);
        Map(m => m.Submetering3).Name(SubmeteringsStatsCsvRecord.Submetering3ColumnKey);
    }
}