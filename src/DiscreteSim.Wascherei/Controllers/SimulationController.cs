using DiscreteSim.Wascherei.Common.Input;
using DiscreteSim.Wascherei.Helpers;
using DiscreteSim.Wascherei.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscreteSim.Wascherei.Controllers;

[Route("simulation")]
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
    [Route("run")]
    public IActionResult Run([FromBody] SimulationFormModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = SimulationHelper.RunComparativeSimulation(request.Parameters, request.Year);
            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
