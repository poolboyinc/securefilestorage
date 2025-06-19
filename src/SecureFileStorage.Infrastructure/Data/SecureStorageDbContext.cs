namespace SecureFileStorage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SecureFileStorage.Core.Entities;

public class SecureStorageDbContext : DbContext
{
    public SecureStorageDbContext(DbContextOptions<SecureStorageDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<SecureFile> SecureFiles { get; set; }
    public DbSet<FilePermission> FilePermissions { get; set; }
    public DbSet<FileAuditLog> FileAuditLogs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<EncryptedKey> EncryptedKeys { get; set; }
    public DbSet<UserAuditLog> UserAuditLogs { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });
        
        modelBuilder.Entity<SecureFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Files)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<FilePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.File)
                .WithMany(f => f.Permissions)
                .HasForeignKey(e => e.FileId);
            entity.HasOne(e => e.User)
                .WithMany() 
                .HasForeignKey(e => e.UserId);
        });
        
        modelBuilder.Entity<FileAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.File)
                .WithMany(f => f.AuditLogs)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Restrict); 
            entity.HasOne(e => e.User)
                .WithMany() 
                .HasForeignKey(e => e.UserId);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
        });
        
        modelBuilder.Entity<UserAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany() 
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        modelBuilder.Entity<EncryptedKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyId).IsRequired();
            entity.HasIndex(e => e.KeyId).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}