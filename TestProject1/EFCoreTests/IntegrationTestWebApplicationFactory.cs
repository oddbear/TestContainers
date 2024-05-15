using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using TestRequestFileType;
using TestRequestFileType.DataAccess;

namespace TestProject1.EFCoreTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _msSqlContainer;

    private IntegrationTestWebApplicationFactory(
        MsSqlContainer msSqlContainer)
    {
        _msSqlContainer = msSqlContainer;

        // This must be set for transactions to work:
        Server.PreserveExecutionContext = true;
    }

    public static async Task<IntegrationTestWebApplicationFactory> CreateFactoryAsync(CancellationToken cancellationToken = default)
    {
        // Postgres 10s-5s vs SQL Server 25s-20s
        var msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2017-latest")
            .Build();

        await msSqlContainer.StartAsync(cancellationToken);

        await MigrateEntityFrameworkCore(msSqlContainer.GetConnectionString(), cancellationToken);

        return new IntegrationTestWebApplicationFactory(msSqlContainer);
    }

    private static async Task MigrateEntityFrameworkCore(string connectionString, CancellationToken cancellationToken)
    {
        var dbContextOptions = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseSqlServer(connectionString, builder =>
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
                .UseSqlServer(_msSqlContainer.GetConnectionString());

            services.AddScoped(provider => optionsBuilder.Options);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }
}