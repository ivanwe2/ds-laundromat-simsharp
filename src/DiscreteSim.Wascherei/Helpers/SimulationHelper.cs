using DiscreteSim.Wascherei.Common.Input;
using DiscreteSim.Wascherei.Services;
using System.Text.Json;

namespace DiscreteSim.Wascherei.Helpers;

public class SimulationHelper
{
    public static string RunComparativeSimulation(LaundryParameters parameters, int year = 2025)
    {
        var simulator = new SimulationService(parameters);

        var resultsWithoutDiscounts = simulator.RunSimulation(year, false);
        var resultsWithDiscounts = simulator.RunSimulation(year, true);

        var result = new
        {
            withoutDiscounts = resultsWithoutDiscounts,
            withDiscounts = resultsWithDiscounts,
            parameters,
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(result, options);
    }
}
