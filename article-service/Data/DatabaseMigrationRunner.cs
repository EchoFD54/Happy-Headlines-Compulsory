using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class DatabaseMigrationRunner
{
    private readonly IConfiguration _configuration;

    public DatabaseMigrationRunner(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void MigrateAllDatabases()
{
    var connectionStrings = new Dictionary<string, string>
    {
        { "Africa", _configuration.GetConnectionString("AfricaDb") },
        { "Antarctica", _configuration.GetConnectionString("AntarcticaDb") },
        { "Asia", _configuration.GetConnectionString("AsiaDb") },
        { "Europe", _configuration.GetConnectionString("EuropeDb") },
        { "America", _configuration.GetConnectionString("AmericaDb") },
        { "Oceania", _configuration.GetConnectionString("OceaniaDb") },
        { "Global", _configuration.GetConnectionString("GlobalDb") }
    };

    foreach (var kvp in connectionStrings)
    {
        Console.WriteLine($"Migrating database for {kvp.Key}...");

        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
        optionsBuilder.UseNpgsql(kvp.Value);

        var retries = 10;

        while (retries > 0)
        {
            try
            {
                using var context = new ArticleDbContext(optionsBuilder.Options);
                context.Database.Migrate();

                Console.WriteLine($"Database for {kvp.Key} migrated successfully!");
                break;
            }
            catch (Exception ex)
            {
                retries--;
                Console.WriteLine($"Database not ready. Retrying in 5s... ({retries} retries left)");
                Thread.Sleep(5000);

                if (retries == 0)
                    throw;
            }
        }
    }
}
}