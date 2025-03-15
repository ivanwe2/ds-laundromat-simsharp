namespace DiscreteSim.Wascherei.Common.Stats;

public record YearlyStats
{
    public double TotalRevenue { get; init; }
    public double TotalCosts { get; init; }
    public double TotalProfit { get; init; }
    public int TotalCustomers { get; init; }
    public int TotalLostCustomers { get; init; }
    public double TotalLostRevenue { get; init; }
    public double AverageMachineUtilization { get; init; }

    public List<MonthlyStats> MonthlyData { get; init; } = [];
    public List<DailyStats> DailyData { get; init; } = [];
}
