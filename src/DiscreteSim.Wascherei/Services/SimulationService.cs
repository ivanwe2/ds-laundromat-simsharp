using SimSharp;
using static SimSharp.Distributions;
using DiscreteSim.Wascherei.Common.Input;
using DiscreteSim.Wascherei.Common.Stats;

namespace DiscreteSim.Wascherei.Services;

public class SimulationService
{
    private readonly LaundryParameters _params;
    private readonly Simulation _env = new();
    private readonly IRandom _rand = new SimSharp.SystemRandom();

    private readonly Normal _spendDistribution;
    private readonly Uniform _variationDistribution;

    public SimulationService(LaundryParameters parameters)
    {
        _params = parameters;
        _spendDistribution = N(_params.AvgSpendPerStudent, _params.StdDevSpend); // Normal(mean, stdDev)
        _variationDistribution = UNIF(0.8, 1.2); // Uniform(min, max)
    }

    public YearlyStats RunSimulation(int year, bool useDiscounts)
    {
        _params.UseDiscounts = useDiscounts;
        var dailyStats = new List<DailyStats>();

        _env.Process(SimulateYear(year, dailyStats));
        _env.Run();

        var monthlyStats = dailyStats.GroupBy(d => d.Month)
            .Select(g => new MonthlyStats
            {
                Month = g.Key,
                MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                TotalRevenue = g.Sum(d => d.Revenue),
                TotalCosts = g.Sum(d => d.Costs),
                TotalProfit = g.Sum(d => d.Profit),
                TotalCustomers = g.Sum(d => d.Customers),
                TotalLostCustomers = g.Sum(d => d.LostCustomers),
                TotalLostRevenue = g.Sum(d => d.LostRevenue),
                AverageMachineUtilization = g.Average(d => d.MachineUtilization)
            }).ToList();

        return new YearlyStats
        {
            TotalRevenue = dailyStats.Sum(d => d.Revenue),
            TotalCosts = dailyStats.Sum(d => d.Costs),
            TotalProfit = dailyStats.Sum(d => d.Profit),
            TotalCustomers = dailyStats.Sum(d => d.Customers),
            TotalLostCustomers = dailyStats.Sum(d => d.LostCustomers),
            TotalLostRevenue = dailyStats.Sum(d => d.LostRevenue),
            AverageMachineUtilization = dailyStats.Average(d => d.MachineUtilization),
            MonthlyData = monthlyStats,
            DailyData = dailyStats
        };
    }

    private IEnumerable<Event> SimulateYear(int year, List<DailyStats> dailyStats)
    {
        DateTime startDate = new(year, 1, 1);
        DateTime endDate = new(year, 12, 31);

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            yield return _env.Process(SimulateDay(date, dailyStats));
        }
    }

    private IEnumerable<Event> SimulateDay(DateTime date, List<DailyStats> dailyStats)
    {
        double monthlyFactor = _params.MonthlyFactors[date.Month];
        bool isWeekend = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        int totalDailyMachineCapacity = (int)Math.Floor(_params.MachinesAvailable * _params.OperatingHours / _params.AvgMachineUsageHours);
        double randomVariation = _variationDistribution.Sample(_rand);

        double dayBoost = isWeekend ? (_params.UseDiscounts ? _params.WeekendBoostWithDiscount : _params.WeekendBoostNoDiscount) : (_params.UseDiscounts ? _params.WeekdayBoostWithDiscount : 1.0);
        int numberOfCustomers = (int)Math.Round(_params.DailyCustomerBaseTarget * dayBoost * monthlyFactor * randomVariation);

        int servedCustomers = Math.Min(numberOfCustomers, totalDailyMachineCapacity);
        int lostCustomers = numberOfCustomers - servedCustomers;

        double machineUtilization = (servedCustomers * _params.AvgMachineUsageHours) / (_params.MachinesAvailable * _params.OperatingHours);
        double priceModifier = _params.UseDiscounts ? _params.DayPriceModifiers[date.DayOfWeek] : 1.0;

        double dailyRevenue = _spendDistribution.Sample(_rand) * servedCustomers * priceModifier;
        double lostRevenue = _spendDistribution.Sample(_rand) * lostCustomers * priceModifier;
        double dailyCosts = _params.FixedCost + (dailyRevenue * _params.VariableCostFactor);
        if (date.DayOfWeek == DayOfWeek.Monday) dailyCosts += _params.MaintenanceCost;

        double dailyProfit = dailyRevenue - dailyCosts;

        dailyStats.Add(new DailyStats
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            Month = date.Month,
            Revenue = dailyRevenue,
            Costs = dailyCosts,
            Profit = dailyProfit,
            Customers = servedCustomers,
            LostCustomers = lostCustomers,
            LostRevenue = lostRevenue,
            MachineUtilization = machineUtilization
        });

        yield return _env.Timeout(TimeSpan.FromDays(1));
    }
}
