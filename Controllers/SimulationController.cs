using DiscreteSim.Wascherei.Common.Input;
using DiscreteSim.Wascherei.Helpers;
using DiscreteSim.Wascherei.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscreteSim.Wascherei.Controllers;

public class SimulationController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new SimulationFormModel
        {
            Parameters = new LaundryParameters()
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult RunSimulation(SimulationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var result = SimulationHelper.RunComparativeSimulation(model.Parameters, model.Year);

        // Pass the JSON result to the Results view
        ViewBag.SimulationResults = result;
        return View("Results");
    }

    [HttpPost]
    public IActionResult GetSimulationData([FromBody] LaundryParameters parameters, int year = 2025)
    {
        try
        {
            var result = SimulationHelper.RunComparativeSimulation(parameters, year);
            return Json(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
