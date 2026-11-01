using Microsoft.EntityFrameworkCore;
using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Data;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Claim Configuration
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToTable("Claims");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.PolicyNumber)
                  .IsRequired()
                  .HasMaxLength(50);

            // Índice en PolicyNumber NO único (una póliza puede acumular múltiples siniestros)
            entity.HasIndex(c => c.PolicyNumber)
                  .IsUnique(false);

            entity.Property(c => c.InsuredName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(c => c.ClaimType)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.EstimatedAmount)
                  .HasPrecision(18, 2);

            entity.Property(c => c.CurrentStatus)
                  .IsRequired()
                  .HasMaxLength(30);

            entity.Property(c => c.CreatedAt)
                  .IsRequired();

            entity.Property(c => c.UpdatedAt)
                  .IsRequired(false);

            // Relación 1:N con ClaimStatusHistory
            entity.HasMany(c => c.StatusHistory)
                  .WithOne(h => h.Claim)
                  .HasForeignKey(h => h.ClaimId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ClaimStatusHistory Configuration
        modelBuilder.Entity<ClaimStatusHistory>(entity =>
        {
            entity.ToTable("ClaimStatusHistories");

            entity.HasKey(h => h.Id);

            // PreviousStatus nullable para registrar el estado inicial (null -> 'abierto')
            entity.Property(h => h.PreviousStatus)
                  .HasMaxLength(30)
                  .IsRequired(false);

            entity.Property(h => h.NewStatus)
                  .IsRequired()
                  .HasMaxLength(30);

            // ChangeReason opcional
            entity.Property(h => h.ChangeReason)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(h => h.ChangedAt)
                  .IsRequired();
        });

        // Seed Data con fechas UTC fijas (determinísticas) e historial coherente
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var fixedDate1 = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var fixedDate2A = new DateTime(2026, 1, 12, 14, 30, 0, DateTimeKind.Utc);
        var fixedDate2B = new DateTime(2026, 1, 13, 10, 15, 0, DateTimeKind.Utc);
        var fixedDate3A = new DateTime(2026, 1, 5, 11, 0, 0, DateTimeKind.Utc);
        var fixedDate3B = new DateTime(2026, 1, 8, 15, 20, 0, DateTimeKind.Utc);
        var fixedDate3C = new DateTime(2026, 1, 20, 16, 45, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Claim>().HasData(
            new Claim
            {
                Id = 1,
                PolicyNumber = "POL-2024-001",
                InsuredName = "Ana Gómez",
                ClaimType = "Colisión vehicular",
                EstimatedAmount = 18500.00m,
                CurrentStatus = "abierto",
                CreatedAt = fixedDate1,
                UpdatedAt = null
            },
            new Claim
            {
                Id = 2,
                PolicyNumber = "POL-2024-002",
                InsuredName = "Carlos Méndez",
                ClaimType = "Robo de autopartes",
                EstimatedAmount = 12000.00m,
                CurrentStatus = "en revisión",
                CreatedAt = fixedDate2A,
                UpdatedAt = fixedDate2B
            },
            new Claim
            {
                Id = 3,
                PolicyNumber = "POL-2024-003",
                InsuredName = "Mariana Torres",
                ClaimType = "Daños por inundación",
                EstimatedAmount = 45000.00m,
                CurrentStatus = "cerrado",
                CreatedAt = fixedDate3A,
                UpdatedAt = fixedDate3C
            }
        );

        modelBuilder.Entity<ClaimStatusHistory>().HasData(
            // Historial Siniestro 1: Estado actual 'abierto' (1 entrada inicial)
            new ClaimStatusHistory
            {
                Id = 1,
                ClaimId = 1,
                PreviousStatus = null,
                NewStatus = "abierto",
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate1
            },
            // Historial Siniestro 2: Estado actual 'en revisión' (2 entradas: abierto -> en revisión)
            new ClaimStatusHistory
            {
                Id = 2,
                ClaimId = 2,
                PreviousStatus = null,
                NewStatus = "abierto",
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate2A
            },
            new ClaimStatusHistory
            {
                Id = 3,
                ClaimId = 2,
                PreviousStatus = "abierto",
                NewStatus = "en revisión",
                ChangeReason = "Asignación a perito técnico para inspección física",
                ChangedAt = fixedDate2B
            },
            // Historial Siniestro 3: Estado actual 'cerrado' (3 entradas: abierto -> en revisión -> cerrado)
            new ClaimStatusHistory
            {
                Id = 4,
                ClaimId = 3,
                PreviousStatus = null,
                NewStatus = "abierto",
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate3A
            },
            new ClaimStatusHistory
            {
                Id = 5,
                ClaimId = 3,
                PreviousStatus = "abierto",
                NewStatus = "en revisión",
                ChangeReason = "Revisión de cobertura de póliza y deducible",
                ChangedAt = fixedDate3B
            },
            new ClaimStatusHistory
            {
                Id = 6,
                ClaimId = 3,
                PreviousStatus = "en revisión",
                NewStatus = "cerrado",
                ChangeReason = "Indemnización liquidada y finiquitada por el asegurado",
                ChangedAt = fixedDate3C
            }
        );
    }
}

