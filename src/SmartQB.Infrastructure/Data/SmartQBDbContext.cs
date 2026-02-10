using Microsoft.EntityFrameworkCore;
using SmartQB.Core.Entities;

namespace SmartQB.Infrastructure.Data;

public class SmartQBDbContext : DbContext
{
    public SmartQBDbContext(DbContextOptions<SmartQBDbContext> options) : base(options)
    {
    }

    public DbSet<Question> Questions { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Many-to-Many relationship between Question and Tag
        modelBuilder.Entity<Question>()
            .HasMany(q => q.Tags)
            .WithMany(t => t.Questions);
    }
}
