using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ArticleDbContextFactory : IDesignTimeDbContextFactory<ArticleDbContext>
{
    public ArticleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();


        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=articles_africa;Username=hhuser;Password=hhpass");

        return new ArticleDbContext(optionsBuilder.Options);
    }
}