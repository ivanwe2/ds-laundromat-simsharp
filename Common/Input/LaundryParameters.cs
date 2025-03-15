namespace DiscreteSim.Wascherei.Common.Input;

public class LaundryParameters
{
    public int MachinesAvailable { get; set; } = 20;

    public int OperatingHours { get; set; } = 24;
    public double AvgMachineUsageHours { get; set; } = 4.0;
    public int DailyCustomerBaseTarget { get; set; } = 100;

    public double AvgSpendPerStudent { get; set; } = 8.0;
    public double StdDevSpend { get; set; } = 2.0;
    public double FixedCost { get; set; } = 100.0;
    public double VariableCostFactor { get; set; } = 0.3;
    public double MaintenanceCost { get; set; } = 150.0;

    public bool UseDiscounts { get; set; } = false;

    public Dictionary<int, double> MonthlyFactors { get; set; } = new Dictionary<int, double>
    {
        { 1, 0.7 },
        { 2, 1.0 },
        { 3, 1.1 },
        { 4, 1.2 },
        { 5, 1.4 },
        { 6, 0.6 },
        { 7, 0.4 },
        { 8, 0.5 },
        { 9, 1.1 },
        { 10, 1.0 },
        { 11, 1.2 },
        { 12, 0.8 },
    };

    public Dictionary<DayOfWeek, double> DayPriceModifiers { get; set; } = new Dictionary<DayOfWeek, double>
    {
        { DayOfWeek.Monday, 0.9 },
        { DayOfWeek.Tuesday, 0.9 },
        { DayOfWeek.Wednesday, 0.9 },
        { DayOfWeek.Thursday, 0.9 },
        { DayOfWeek.Friday, 0.8 },
        { DayOfWeek.Saturday, 1.0 },
        { DayOfWeek.Sunday, 1.0 }
    };

    public double WeekdayBoostWithDiscount { get; set; } = 1.10;
    public double WeekendBoostWithDiscount { get; set; } = 1.2;
    public double WeekendBoostNoDiscount { get; set; } = 1.4;
}
