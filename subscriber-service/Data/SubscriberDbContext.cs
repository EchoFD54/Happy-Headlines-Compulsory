using Microsoft.EntityFrameworkCore;

public class SubscriberDbContext : DbContext
{
    public SubscriberDbContext(DbContextOptions<SubscriberDbContext> options)
        : base(options) { }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Subscriber>()
            .HasIndex(s => s.Email)
            .IsUnique();
    }
}