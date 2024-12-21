using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Personal.Domain.Entities;

namespace Personal.Domain.Config;

public class LayoutContext(DbContextOptions<LayoutContext> options) : DbContext(options)
{
    public DbSet<Layout> Layouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Layout>().ToCollection("Layouts");
    }
}
