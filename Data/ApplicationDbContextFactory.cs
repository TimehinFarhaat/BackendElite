using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        Console.WriteLine($"🌍 Env from factory: {env}");

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .Build();

        var connName = env == "Production" ? "PostgresConnection" : "DefaultConnection";
        var connectionString = config.GetConnectionString(connName);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (env == "Production")
        {
            optionsBuilder.UseNpgsql(connectionString,
                o => o.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString,
                o => o.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
