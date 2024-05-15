using TestRequestFileType.DataAccess.Entities;

namespace TestRequestFileType.DataAccess;

public class WeatherRepository
{
    private readonly WeatherDbContext _customerDbContext;

    public WeatherRepository(
        WeatherDbContext customerDbContext)
    {
        _customerDbContext = customerDbContext;
    }

    public IEnumerable<Weather> GetWeathers()
    {
        return _customerDbContext
            .Weathers
            .ToArray();
    }
}