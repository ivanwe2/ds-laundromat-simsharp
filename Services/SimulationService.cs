using DiscreteSim.Wascherei.Common.Input;
using DiscreteSim.Wascherei.Common.Stats;
using SimSharp;

namespace DiscreteSim.Wascherei.Services;

public class SimulationService(LaundryParameters parameters)
{
    private readonly LaundryParameters _params = parameters;
    private readonly Random _random = new();

    public YearlyStats RunSimulation(int year, bool useDiscounts)
    {
        var env = new Simulation();
        _params.UseDiscounts = useDiscounts;

        var dailyStats = new List<DailyStats>();

        // Create a process for each day of the year
        DateTime startDate = new DateTime(year, 1, 1);
        DateTime endDate = new DateTime(year, 12, 31);

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var stats = SimulateDay(date);
            dailyStats.Add(stats);
        }

        // Calculate monthly and yearly aggregates
        var monthlyStats = dailyStats
            .GroupBy(d => d.Month)
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
            })
            .ToList();

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

    private DailyStats SimulateDay(DateTime date)
    {
        // Get monthly seasonality factor
        double monthlyFactor = _params.MonthlyFactors[date.Month];

        // Get day of week and determine if it's a weekend
        bool isWeekend = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);

        // Calculate total machine capacity for the day
        int totalDailyMachineCapacity = (int)Math.Floor(_params.MachinesAvailable * _params.OperatingHours / _params.AvgMachineUsageHours);

        // Random daily variation (80%-120% of base)
        double randomVariation = 0.8 + _random.NextDouble() * 0.4;

        // Calculate expected number of customers based on pricing strategy, day, and season
        int numberOfCustomers;
        if (_params.UseDiscounts)
        {
            double dayBoost = isWeekend ? _params.WeekendBoostWithDiscount : _params.WeekdayBoostWithDiscount;
            numberOfCustomers = (int)Math.Round(_params.DailyCustomerBaseTarget * dayBoost * monthlyFactor * randomVariation);
        }
        else
        {
            double dayBoost = isWeekend ? _params.WeekendBoostNoDiscount : 1.0;
            numberOfCustomers = (int)Math.Round(_params.DailyCustomerBaseTarget * dayBoost * monthlyFactor * randomVariation);
        }

        // Calculate served vs lost customers
        int servedCustomers = Math.Min(numberOfCustomers, totalDailyMachineCapacity);
        int lostCustomers = Math.Max(0, numberOfCustomers - servedCustomers);

        // Calculate machine utilization
        double machineUtilization = (servedCustomers * _params.AvgMachineUsageHours) / (_params.MachinesAvailable * _params.OperatingHours);

        // Calculate revenue and lost revenue
        double priceModifier = _params.UseDiscounts ? _params.DayPriceModifiers[date.DayOfWeek] : 1.0;
        double dailyRevenue = 0;
        double lostRevenue = 0;

        for (int i = 0; i < servedCustomers; i++)
        {
            double spendAmount = GetRandomSpend();
            dailyRevenue += spendAmount * priceModifier;
        }

        for (int i = 0; i < lostCustomers; i++)
        {
            double spendAmount = GetRandomSpend();
            lostRevenue += spendAmount * priceModifier;
        }

        // Calculate costs
        double dailyCosts = _params.FixedCost + (dailyRevenue * _params.VariableCostFactor);

        // Add maintenance cost on Mondays
        if (date.DayOfWeek == DayOfWeek.Monday)
        {
            dailyCosts += _params.MaintenanceCost;
        }

        // Calculate profit
        double dailyProfit = dailyRevenue - dailyCosts;

        return new DailyStats
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
        };
    }

    private double GetRandomSpend()
    {
        // Box-Muller transform to generate normally distributed random numbers
        double u1 = 1.0 - _random.NextDouble();
        double u2 = 1.0 - _random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return _params.AvgSpendPerStudent + _params.StdDevSpend * randStdNormal;
    }
}
