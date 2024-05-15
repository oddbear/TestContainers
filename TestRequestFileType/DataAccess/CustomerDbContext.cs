using Microsoft.EntityFrameworkCore;

namespace TestRequestFileType.DataAccess;

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
        //
    }
}
