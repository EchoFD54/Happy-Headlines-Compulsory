using Microsoft.EntityFrameworkCore;

public class ProfanityDbContext : DbContext
{
    public ProfanityDbContext(DbContextOptions<ProfanityDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProfanityWord> ProfanityWords { get; set; }
}