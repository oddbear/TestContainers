using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TestRequestFileType.DataAccess;

namespace TestProject1.EFCoreTests;

public sealed class CustomerServiceTest
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    private CustomerDbContext _dbContext;

    [SetUp]
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var optionsBuilder = new DbContextOptionsBuilder<CustomerDbContext>();

        optionsBuilder
            .UseNpgsql(_postgres.GetConnectionString(),
                builder =>
                {
                    var dbContextAssemblyName = typeof(CustomerDbContext).Assembly.GetName().FullName;
                    builder.MigrationsAssembly(dbContextAssemblyName);
                }
            );

        _dbContext = new CustomerDbContext(optionsBuilder.Options);
        await _dbContext.Database.MigrateAsync();
    }

    [TearDown]
    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    [Test]
    public void ShouldReturnTwoCustomers()
    {
        // Arrange
        var customerService = new CustomerRepository(_dbContext);

        // Act
        customerService.Create(new Customer { Id = 1, Name = "George" });
        customerService.Create(new Customer { Id = 2, Name = "John" });
        var customers = customerService.GetCustomers();

        // Assert
        Assert.That(customers.Count(), Is.EqualTo(2));
    }
}
