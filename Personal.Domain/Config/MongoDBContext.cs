using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Personal.Domain.Entities;

namespace Personal.Domain.Config;

public class MongoDBContext(DbContextOptions<MongoDBContext> options) : DbContext(options)
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<ReadPaging> ReadPagings { get; set; }

    public DbSet<BookPartition> BookPartitions { get; set; }
    public DbSet<Genre> Genres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Country>().ToCollection("Countries");
        modelBuilder.Entity<Author>().ToCollection("Authors");
        modelBuilder.Entity<Book>().ToCollection("Books");
        modelBuilder.Entity<ReadPaging>().ToCollection("ReadPaging");
        modelBuilder.Entity<BookPartition>().ToCollection("BookPartitions");
        modelBuilder.Entity<Genre>().ToCollection("Genres");
    }
}
