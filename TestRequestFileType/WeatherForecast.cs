using TestRequestFileType.ExcelOutputFormatter.Attributes;

namespace TestRequestFileType;

[ExcelFile(FileName = "WeatherForecast.xlsx")]
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
