namespace DiscreteSim.Wascherei.Common.Stats;

public record MonthlyStats
{
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public double TotalRevenue { get; init; }
    public double TotalCosts { get; init; }
    public double TotalProfit { get; init; }
    public int TotalCustomers { get; init; }
    public int TotalLostCustomers { get; init; }
    public double TotalLostRevenue { get; init; }
    public double AverageMachineUtilization { get; init; }
}
