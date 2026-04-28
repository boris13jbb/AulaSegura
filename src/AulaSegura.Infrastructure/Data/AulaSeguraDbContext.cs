using Microsoft.EntityFrameworkCore;
using AulaSegura.Core.Entities;

namespace AulaSegura.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para AulaSegura
/// </summary>
public class AulaSeguraDbContext : DbContext
{
    public DbSet<Administrator> Administrators { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<BlockedSite> BlockedSites { get; set; } = null!;
    public DbSet<AllowedSite> AllowedSites { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<Backup> Backups { get; set; } = null!;
    public DbSet<Keyword> Keywords { get; set; } = null!;
    public DbSet<BlockingRule> BlockingRules { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;

    public AulaSeguraDbContext(DbContextOptions<AulaSeguraDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Administrator
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configuración de Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configuración de BlockedSite
        modelBuilder.Entity<BlockedSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasOne(e => e.Category).WithMany(c => c.BlockedSites).HasForeignKey(e => e.CategoryId);
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        // Configuración de AllowedSite
        modelBuilder.Entity<AllowedSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        // Configuración de Schedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Configuración de ActivityLog
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EntityType).HasMaxLength(50);
        });

        // Configuración de Setting
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // Configuración de Backup
        modelBuilder.Entity<Backup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BackupPath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BackupType).HasMaxLength(50);
        });

        // Configuración de Keyword
        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
        });

        // Configuración de BlockingRule
        modelBuilder.Entity<BlockingRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(2000);
            entity.Property(e => e.Action).HasMaxLength(50);
        });

        // Configuración de Report
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.ClientIp).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ActionTaken).HasMaxLength(50);
            entity.HasOne(e => e.Administrator).WithMany().HasForeignKey(e => e.AdministratorId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
