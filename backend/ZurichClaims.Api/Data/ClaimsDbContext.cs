using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

        // Value converters para garantizar que SQLite devuelva siempre DateTime con Kind=Utc
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue
                ? v
                : (v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
            v => !v.HasValue ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
        );

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
                  .HasConversion<string>()
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(c => c.CreatedAt)
                  .HasConversion(utcConverter)
                  .IsRequired();

            entity.Property(c => c.UpdatedAt)
                  .HasConversion(nullableUtcConverter)
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

            // PreviousStatus nullable para registrar el estado inicial (null -> Open)
            entity.Property(h => h.PreviousStatus)
                  .HasConversion<string>()
                  .HasMaxLength(30)
                  .IsRequired(false);

            entity.Property(h => h.NewStatus)
                  .HasConversion<string>()
                  .HasMaxLength(30)
                  .IsRequired();

            // ChangeReason opcional
            entity.Property(h => h.ChangeReason)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(h => h.ChangedAt)
                  .HasConversion(utcConverter)
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
                CurrentStatus = ClaimStatus.Open,
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
                CurrentStatus = ClaimStatus.InReview,
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
                CurrentStatus = ClaimStatus.Closed,
                CreatedAt = fixedDate3A,
                UpdatedAt = fixedDate3C
            }
        );

        modelBuilder.Entity<ClaimStatusHistory>().HasData(
            // Historial Siniestro 1: Estado actual Open (1 entrada inicial)
            new ClaimStatusHistory
            {
                Id = 1,
                ClaimId = 1,
                PreviousStatus = null,
                NewStatus = ClaimStatus.Open,
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate1
            },
            // Historial Siniestro 2: Estado actual InReview (2 entradas: Open -> InReview)
            new ClaimStatusHistory
            {
                Id = 2,
                ClaimId = 2,
                PreviousStatus = null,
                NewStatus = ClaimStatus.Open,
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate2A
            },
            new ClaimStatusHistory
            {
                Id = 3,
                ClaimId = 2,
                PreviousStatus = ClaimStatus.Open,
                NewStatus = ClaimStatus.InReview,
                ChangeReason = "Asignación a perito técnico para inspección física",
                ChangedAt = fixedDate2B
            },
            // Historial Siniestro 3: Estado actual Closed (3 entradas: Open -> InReview -> Closed)
            new ClaimStatusHistory
            {
                Id = 4,
                ClaimId = 3,
                PreviousStatus = null,
                NewStatus = ClaimStatus.Open,
                ChangeReason = "Apertura y registro inicial del siniestro",
                ChangedAt = fixedDate3A
            },
            new ClaimStatusHistory
            {
                Id = 5,
                ClaimId = 3,
                PreviousStatus = ClaimStatus.Open,
                NewStatus = ClaimStatus.InReview,
                ChangeReason = "Revisión de cobertura de póliza y deducible",
                ChangedAt = fixedDate3B
            },
            new ClaimStatusHistory
            {
                Id = 6,
                ClaimId = 3,
                PreviousStatus = ClaimStatus.InReview,
                NewStatus = ClaimStatus.Closed,
                ChangeReason = "Indemnización liquidada y finiquitada por el asegurado",
                ChangedAt = fixedDate3C
            }
        );
    }
}

