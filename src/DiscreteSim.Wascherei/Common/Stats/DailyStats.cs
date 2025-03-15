namespace DiscreteSim.Wascherei.Common.Stats;

public record DailyStats
{
    public DateTime Date { get; init; }
    public string DayOfWeek { get; init; } = string.Empty;
    public int Month { get; init; }
    public double Revenue { get; init; }
    public double Costs { get; init; }
    public double Profit { get; init; }
    public int Customers { get; init; }
    public int LostCustomers { get; init; }
    public double LostRevenue { get; init; }
    public double MachineUtilization { get; init; }
}
