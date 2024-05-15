namespace TestRequestFileType.DataAccess;

public class CustomerRepository
{
    private readonly CustomerDbContext _customerDbContext;

    public CustomerRepository(
        CustomerDbContext customerDbContext)
    {
        _customerDbContext = customerDbContext;
    }

    public IEnumerable<Customer> GetCustomers()
    {
        return _customerDbContext
            .Customers
            .ToArray();
    }

    public void Create(Customer customer)
    {
        _customerDbContext.Add(customer);
        _customerDbContext.SaveChanges();
    }
}