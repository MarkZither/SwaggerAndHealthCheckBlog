using ChaosResilientService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChaosResilientService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .ToTable("Team")
                // don't do this in a real system due to the limitation https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding#limitations-of-model-seed-data
                // instead use custom initialization logic
                // https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding#custom-initialization-logic
                //.HasData(
                //    new Team() { TeamId = 1, Url = "https://blahblah"},
                //    new Team() { TeamId = 2, Url = "https://foobar" }
                //)
                ;
            base.OnModelCreating(modelBuilder);
        }
    }
}
