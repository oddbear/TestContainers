using TestRequestFileType.ExcelOutputFormatter.Attributes;

namespace TestRequestFileType;


public class WeatherForecast
{
    [ExcelColumn]
    public DateOnly Date { get; set; }

    [ExcelColumn]
    public int TemperatureC { get; set; }

    [ExcelColumn]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [ExcelColumn]
    public string? Summary { get; set; }
}
