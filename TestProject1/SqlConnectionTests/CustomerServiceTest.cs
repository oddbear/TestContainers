using System.Transactions;
using Testcontainers.PostgreSql;

namespace TestProject1.SqlConnectionTests;

// [Parallelizable] defaults to per-fixture (class).
// It's also possible to use the [SetUpFixture] to start up globaly (assembly wide).
// For performance use a transactionscope per test and rollback, rather than one container per test.
[Parallelizable] public class CustomerServiceTest1 : CustomerServiceTest { }
[Parallelizable] public class CustomerServiceTest2 : CustomerServiceTest { }

// WARNING: Never do this abstract trick, it's just to simulate parallelism.
public abstract class CustomerServiceTest
{
    private PostgreSqlContainer _postgres;
    private TransactionScope _transaction;

    [OneTimeSetUp]
    public Task OneTimeSetup()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        return _postgres.StartAsync();
    }

    [OneTimeTearDown]
    public Task OneTimeTearDown()
    {
        return _postgres.DisposeAsync().AsTask();
    }


    [SetUp]
    public void Setup()
    {
        // If you remove this transaction, only one test per fixture will be OK.
        // This is because they are running in different, and clean containers (next test is dirty).
        _transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
    }

    [TearDown]
    public void Dispose()
    {
        _transaction.Dispose();
    }

    [TestCaseSource(nameof(DummyData))]
    public void ShouldReturnTwoCustomers(int dummyData)
    {
        // Arrange
        var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));

        // Act
        customerService.Create(new Customer(1, "George"));
        customerService.Create(new Customer(2, "John"));
        var customers = customerService.GetCustomers();

        // Assert
        Assert.That(customers.Count(), Is.EqualTo(2));
    }

    static IEnumerable<int> DummyData()
        => Enumerable.Range(1, 50).ToArray();
}
