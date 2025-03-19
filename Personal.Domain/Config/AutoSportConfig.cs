using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Personal.Domain.Entities.MotorSport;

namespace Personal.Domain.Config;

public class MotoSportContext(DbContextOptions<MotoSportContext> options) : DbContext(options)
{
    public DbSet<Constructor> Constructor { get; set; }
    public DbSet<Driver> Driver { get; set; }
    public DbSet<PowerUnit> PowerUnit { get; set; }
    public DbSet<RaceEvent> RaceEvent { get; set; }
    public DbSet<RaceTrack> RaceTrack { get; set; }
    public DbSet<RacingTeam> RacingTeam { get; set; }
    public DbSet<Season> Season { get; set; }
    public DbSet<MotorSportType> MotorSportType { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Constructor>().ToCollection("Constructor");
        modelBuilder.Entity<Driver>().ToCollection("Driver");
        modelBuilder.Entity<PowerUnit>().ToCollection("PowerUnit");
        modelBuilder.Entity<RaceEvent>().ToCollection("RaceEvent");
        modelBuilder.Entity<RaceTrack>().ToCollection("RaceTrack");
        modelBuilder.Entity<RacingTeam>().ToCollection("RacingTeam");
        modelBuilder.Entity<Season>().ToCollection("Season");
        modelBuilder.Entity<MotorSportType>().ToCollection("MotorSportType");
    }
}
