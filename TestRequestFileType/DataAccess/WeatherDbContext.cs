using Microsoft.EntityFrameworkCore;
using TestRequestFileType.DataAccess.Entities;

namespace TestRequestFileType.DataAccess;

public class WeatherDbContext : DbContext
{
    public DbSet<Weather> Weathers => Set<Weather>();

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
        //
    }
}
