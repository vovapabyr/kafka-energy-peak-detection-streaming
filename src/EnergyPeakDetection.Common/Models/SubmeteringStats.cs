﻿namespace EnergyPeakDetection.Common;

public class SubmeteringStats
{
    public string Key { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public double Value { get; set; }

    public DateTime GetDateTime() => Date.Add(Time);

    public string GetKey() => $"{ Key }|{ GetDateTime() }";

    public override string ToString()
    {
        return $"{ Key }|{ Date }|{ Time }|{ Value }"; 
    }
}
