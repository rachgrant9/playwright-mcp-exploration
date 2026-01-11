using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Web.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.IsCompleted)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });
    }
}
