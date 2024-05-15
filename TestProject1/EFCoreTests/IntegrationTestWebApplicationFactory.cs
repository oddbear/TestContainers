using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using TestRequestFileType;
using TestRequestFileType.DataAccess;

namespace TestProject1.EFCoreTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres;

    private IntegrationTestWebApplicationFactory(
        PostgreSqlContainer postgres)
    {
        _postgres = postgres;
    }

    public static async Task<IntegrationTestWebApplicationFactory> CreateFactoryAsync(CancellationToken cancellationToken = default)
    {
        var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        await postgres.StartAsync(cancellationToken);

        await MigrateEntityFrameworkCore(postgres.GetConnectionString(), cancellationToken);

        return new IntegrationTestWebApplicationFactory(postgres);
    }

    private static async Task MigrateEntityFrameworkCore(string connectionString, CancellationToken cancellationToken)
    {
        var dbContextOptions = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseNpgsql(connectionString, builder =>
            {
                // This is located in another project:
                var dbContextAssemblyName = typeof(WeatherDbContext).Assembly.GetName().FullName;
                builder.MigrationsAssembly(dbContextAssemblyName);
            })
            .Options;

        using var migrationDbContext = new WeatherDbContext(dbContextOptions);
        await migrationDbContext.Database.MigrateAsync(cancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the DbContext options to point at the TestContainer:
            services.RemoveAll<DbContextOptions<WeatherDbContext>>();

            // We don't include the migration here:
            var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseNpgsql(_postgres.GetConnectionString());

            services.AddScoped(provider => optionsBuilder.Options);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}