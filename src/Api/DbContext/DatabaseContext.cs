using Domain;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class DatabaseContext : DbContext
{
    protected DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>(x =>
        {
            x.HasKey(e => e.Shorten);

            x.Property(e => e.Original);
            x.Property(e => e.Shorten);
            x.Property(e => e.Expiry);
        });
    }

    public DbSet<Url> Urls { get; set; } = null!;
}