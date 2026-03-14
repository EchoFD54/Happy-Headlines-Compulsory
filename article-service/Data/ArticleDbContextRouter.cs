using Microsoft.EntityFrameworkCore;

public class ArticleDbContextRouter : IArticleDbContextRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ArticleDbContextRouter(IServiceProvider serviceProvider, IConfiguration configuration){
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public ArticleDbContext GetDbContext(Continent continent){
        string connectionString = continent switch{
            Continent.Africa => _configuration.GetConnectionString("AfricaDb"),
            Continent.Antarctica => _configuration.GetConnectionString("AntarcticaDb"),
            Continent.Asia => _configuration.GetConnectionString("AsiaDb"),
            Continent.Europe => _configuration.GetConnectionString("EuropeDb"),
            Continent.America => _configuration.GetConnectionString("AmericaDb"),
            Continent.Oceania => _configuration.GetConnectionString("OceaniaDb"),
            Continent.Global => _configuration.GetConnectionString("GlobalDb"),
            _ => throw new ArgumentException("Invalid continent")
        };

        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ArticleDbContext(optionsBuilder.Options);
    }
}