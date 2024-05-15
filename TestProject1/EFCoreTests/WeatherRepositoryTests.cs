using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using TestRequestFileType;
using TestRequestFileType.Controllers;
using TestRequestFileType.DataAccess;
using TestRequestFileType.DataAccess.Entities;

namespace TestProject1.EFCoreTests;

public sealed class WeatherRepositoryTests
{
    private IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public async Task SetupAsync()
    {
        _factory = await IntegrationTestWebApplicationFactory.CreateFactoryAsync();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task JsonResultTest()
    {
        // Arrange
        await PopulateDatabase();

        // Act
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        var result = await _client.GetAsync("weatherforecast");
        var jsonResult = await result.Content.ReadAsStringAsync();
        
        var customers = JsonSerializer.Deserialize<WeatherForecast[]>(jsonResult);

        // Assert
        Assert.That(result.Content.Headers.ContentType?.MediaType, Is.EqualTo(MediaTypeNames.Application.Json));
        Assert.That(customers, Is.Not.Null);
        Assert.That(customers.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task ExcelResultTest()
    {
        // Arrange
        await PopulateDatabase();

        // Act
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ExcelModern));
        var result = await _client.GetAsync("weatherforecast");
        var excelResult = await result.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.That(result.Content.Headers.ContentDisposition?.FileName, Is.EqualTo("\"WeatherForecast.xlsx\""));
        Assert.That(result.Content.Headers.ContentType?.MediaType, Is.EqualTo(MediaTypeConstants.ExcelModern));
        Assert.That(excelResult, Is.Not.Null);
        Assert.That(excelResult.Length, Is.GreaterThan(0));
    }

    private async Task PopulateDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        
        var weatherDbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        
        weatherDbContext.Add(new Weather { Date = new DateOnly(2010, 05, 17), TemperatureC = 26, Summary = "Skikkelig godt vær." });
        weatherDbContext.Add(new Weather { Date = new DateOnly(2010, 05, 18), TemperatureC = 12, Summary = "Kjip dag med regn, hold deg innendørs." });
        
        await weatherDbContext.SaveChangesAsync();
    }
}
