using Microsoft.EntityFrameworkCore;
using ZgjedhjetApi.Models.Entities;

namespace ZgjedhjetApi.Data
{
    // YOUR CODE HERE
    public class LifeDbContext : DbContext
    {
        public LifeDbContext(DbContextOptions<LifeDbContext> options) : base(options)
        {
        }

        public DbSet<Zgjedhjet> Zgjedhjet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Zgjedhjet>(entity =>
            {
                entity.ToTable("Zgjedhjet");
                entity.HasKey(e => e.Id);
            });
        }
    }
}
