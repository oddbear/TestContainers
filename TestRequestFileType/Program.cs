using Microsoft.EntityFrameworkCore;
using TestRequestFileType.DataAccess;
using TestRequestFileType.ExcelOutputFormatter;

namespace TestRequestFileType;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql("connectionString")); // Dummy connectionstring for now (to be able to generate migrations without empty constructor).

        builder.Services.AddControllers(options =>
        {
            //options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
            //options.Filters.Add(new ProducesAttribute("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));options =>
            
            options.OutputFormatters.Add(new ExcelSerializerOutputFormatter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
