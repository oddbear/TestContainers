using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Transactions;
using TestRequestFileType;
using TestRequestFileType.Controllers;
using TestRequestFileType.DataAccess;
using TestRequestFileType.DataAccess.Entities;

namespace TestProject1.EFCoreTests;

[Parallelizable]
public sealed class WeatherRepositoryTests
{
    private IntegrationTestWebApplicationFactory _factory;
    private HttpClient _client;
    private TransactionScope _transaction;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _factory = await IntegrationTestWebApplicationFactory.CreateFactoryAsync();
    }

    [SetUp]
    public void Setup()
    {
        _client = _factory.CreateClient();

        // Use transaction to rollback test data, or similar per test (there are multiple limits on max containers).
        _transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
    }

    [TearDown]
    public void TearDown()
    {
        _transaction.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
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

        var customers = JsonSerializer.Deserialize<WeatherForecast[]>(jsonResult, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,

        });

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
