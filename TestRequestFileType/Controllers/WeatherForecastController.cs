using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TestRequestFileType.DataAccess;

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
    private readonly WeatherRepository _weatherRepository;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        WeatherRepository weatherRepository)
    {
        _logger = logger;
        _weatherRepository = weatherRepository;
    }

    private WeatherForecast[] GetItems()
        => _weatherRepository
        .GetWeathers()
        .Select(weather => new WeatherForecast
        {
            Date = weather.Date,
            TemperatureC = weather.TemperatureC,
            Summary = weather.Summary
        })
        .ToArray();

    private WeatherForecast[] GetItemsLocal()
        => Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

    // JSON
    [HttpGet(Name = "GetWeatherForecast")]
    [Produces(MediaTypeNames.Application.Json, MediaTypeConstants.ExcelModern, Type = typeof(List<WeatherForecast>))]
    public IActionResult Get()
    {
        var items = GetItems();

        //new JsonResult
        return Ok(items);
    }

    //$"user_customer_login_activity_report_{DateTime.Now:yy-MM-dd_HH-mm-ss}.xlsx"
}
