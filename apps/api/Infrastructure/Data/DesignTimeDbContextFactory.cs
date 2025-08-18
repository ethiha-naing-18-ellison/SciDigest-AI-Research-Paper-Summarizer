using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Api.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ResearchDbContext>
{
    public ResearchDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        var conn = config.GetConnectionString("Default")
                   ?? "Server=localhost,1433;Database=ResearchDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ResearchDbContext>()
            .UseSqlServer(conn);

        return new ResearchDbContext(optionsBuilder.Options);
    }
}
