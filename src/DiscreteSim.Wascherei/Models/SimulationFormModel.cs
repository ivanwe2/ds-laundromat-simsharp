using DiscreteSim.Wascherei.Common.Input;
using System.ComponentModel.DataAnnotations;

namespace DiscreteSim.Wascherei.Models;

public class SimulationFormModel
{
    [Required]
    public required LaundryParameters Parameters { get; set; }

    [Range(2024, 2100, ErrorMessage = "Please enter a valid year.")]
    public int Year { get; set; } = DateTime.Now.Year;
}
