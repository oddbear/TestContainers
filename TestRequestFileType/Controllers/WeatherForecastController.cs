using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace TestRequestFileType.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    private WeatherForecast[] GetItems()
        => Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

    // JSON
    [HttpGet(Name = "GetWeatherForecast")]
    [Produces(MediaTypeNames.Application.Json, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Type = typeof(List<WeatherForecast>))]
    public IActionResult Get()
    {
        var items = GetItems();

        //new JsonResult
        return Ok(items);
    }

    //$"user_customer_login_activity_report_{DateTime.Now:yy-MM-dd_HH-mm-ss}.xlsx"
}
