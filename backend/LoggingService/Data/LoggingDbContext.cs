// File: backend/LoggingService/Data/LoggingDbContext.cs
using Microsoft.EntityFrameworkCore;
using LoggingService.Models;

namespace LoggingService.Data;

/// <summary>
/// Database connectivity (Azure SQL)
/// </summary>
public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<Transmission> Transmissions { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Digest> Digests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId).HasMaxLength(50);
            entity.Property(e => e.PatientId).HasMaxLength(50);
            entity.Property(e => e.ClinicId).HasMaxLength(50);
            entity.Property(e => e.MessageType).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.OriginalFhirContent).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ConvertedHl7Content).HasColumnType("nvarchar(max)");
            
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ClinicId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Transmission>(entity =>
        {
            entity.HasKey(e => e.TransmissionId);
            entity.Property(e => e.TransmissionId).HasMaxLength(50);
            entity.Property(e => e.MessageId).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.AcknowledgmentType).HasMaxLength(10);
            entity.Property(e => e.ErrorDetails).HasColumnType("nvarchar(max)");
            
            entity.HasIndex(e => e.MessageId);
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId);
            entity.Property(e => e.ClinicId).HasMaxLength(50);
            entity.Property(e => e.ClinicName).HasMaxLength(100);
            entity.Property(e => e.Settings).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Digest>(entity =>
        {
            entity.HasKey(e => e.DigestId);
            entity.Property(e => e.DigestId).HasMaxLength(50);
        });
    }
}
