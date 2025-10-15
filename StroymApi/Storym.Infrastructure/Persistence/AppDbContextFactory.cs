using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Storym.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Try to locate the API project's appsettings.json at design time.
        var basePath = Directory.GetCurrentDirectory();
        // When run from Infrastructure, this points to ../Storym.Api
        var apiPath = Path.GetFullPath(Path.Combine(basePath, "..", "Storym.Api"));

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiPath) ? apiPath : basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = config.GetConnectionString("Default")
                   ?? "Server=.;Database=Storym;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(conn)
            .Options;

        return new AppDbContext(options);
    }
}
