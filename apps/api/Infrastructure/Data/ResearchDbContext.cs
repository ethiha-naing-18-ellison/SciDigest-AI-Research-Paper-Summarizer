using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public class ResearchDbContext : DbContext
{
    public ResearchDbContext(DbContextOptions<ResearchDbContext> options) : base(options)
    {
    }

    public DbSet<Paper> Papers => Set<Paper>();
    public DbSet<PaperSection> PaperSections => Set<PaperSection>();
    public DbSet<Summary> Summaries => Set<Summary>();
    public DbSet<Contribution> Contributions => Set<Contribution>();
    public DbSet<RelatedWork> RelatedWorks => Set<RelatedWork>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Paper configuration
        modelBuilder.Entity<Paper>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<byte>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes
            entity.HasIndex(e => new { e.Year, e.Venue });
        });

        // PaperSection configuration
        modelBuilder.Entity<PaperSection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Paper)
                  .WithMany(p => p.Sections)
                  .HasForeignKey(e => e.PaperId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique index on PaperId and OrderIdx
            entity.HasIndex(e => new { e.PaperId, e.OrderIdx }).IsUnique();
        });

        // Summary configuration
        modelBuilder.Entity<Summary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Paper)
                  .WithMany(p => p.Summaries)
                  .HasForeignKey(e => e.PaperId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Contribution configuration
        modelBuilder.Entity<Contribution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Paper)
                  .WithMany(p => p.Contributions)
                  .HasForeignKey(e => e.PaperId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // RelatedWork configuration
        modelBuilder.Entity<RelatedWork>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Paper)
                  .WithMany(p => p.RelatedWorks)
                  .HasForeignKey(e => e.PaperId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Index on PaperId and Provider
            entity.HasIndex(e => new { e.PaperId, e.Provider });
        });

        // ProcessingJob configuration
        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Paper)
                  .WithMany(p => p.ProcessingJobs)
                  .HasForeignKey(e => e.PaperId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Index on PaperId, Stage, and State
            entity.HasIndex(e => new { e.PaperId, e.Stage, e.State });
        });
    }
}
