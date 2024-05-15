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

        // TODO: Dummy connectionstring for now (to be able to generate migrations without empty constructor).
        builder.Services.AddDbContext<WeatherDbContext>(options =>
            options.UseNpgsql("connectionString"));

        builder.Services.AddTransient<WeatherRepository>();

        builder.Services.AddControllers(options =>
        {
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
