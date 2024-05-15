using Testcontainers.PostgreSql;

namespace TestProject1.DbConnection;


public sealed class CustomerServiceTest
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    [SetUp]
    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    [TearDown]
    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    [Test]
    public void ShouldReturnTwoCustomers()
    {
        // Given
        var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));

        // When
        customerService.Create(new Customer(1, "George"));
        customerService.Create(new Customer(2, "John"));
        var customers = customerService.GetCustomers();

        // Then
        Assert.That(customers.Count(), Is.EqualTo(2));
    }
}
