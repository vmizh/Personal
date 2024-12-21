using Microsoft.EntityFrameworkCore;

namespace Personal.Domain.Config;

public class AutoSportContext(DbContextOptions<AutoSportContext> options) : DbContext(options)
{
    //public DbSet<Layout> Layouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.Entity<Layout>().ToCollection("Layouts");
    }
}
