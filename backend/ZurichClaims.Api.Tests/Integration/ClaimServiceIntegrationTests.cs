using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZurichClaims.Api.Dtos;
using ZurichClaims.Api.Exceptions;
using ZurichClaims.Api.Models.Entities;
using ZurichClaims.Api.Services;
using ZurichClaims.Api.Tests.Helpers;

namespace ZurichClaims.Api.Tests.Integration;

public class ClaimServiceIntegrationTests
{
    [Fact]
    public async Task CreateAsync_DatosValidos_CreaSiniestroEnEstadoOpenConAuditoriaEHistorial()
    {
        // Arrange
        using var testDb = new TestDb();
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var request = new CreateClaimRequest
        {
            PolicyNumber = "POL-999",
            InsuredName = "María López",
            ClaimType = "Cristal roto",
            EstimatedAmount = 5000.50m
        };
        var userId = "12345";

        // Act
        ClaimDetailResponse created;
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            created = await service.CreateAsync(request, userId);
        }
        var despues = DateTime.UtcNow.AddSeconds(1);

        // Assert
        using (var assertContext = testDb.CreateDbContext())
        {
            var claimDb = await assertContext.Claims
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.Id == created.Id);

            Assert.NotNull(claimDb);
            Assert.Equal("POL-999", claimDb.PolicyNumber);
            Assert.Equal("María López", claimDb.InsuredName);
            Assert.Equal("Cristal roto", claimDb.ClaimType);
            Assert.Equal(5000.50m, claimDb.EstimatedAmount);
            Assert.Equal(ClaimStatus.Open, claimDb.CurrentStatus);
            Assert.Equal(userId, claimDb.CreatedBy);
            Assert.Null(claimDb.UpdatedAt);
            Assert.Null(claimDb.UpdatedBy);
            Assert.False(claimDb.IsDeleted);
            Assert.Null(claimDb.DeletedAt);
            Assert.Null(claimDb.DeletedBy);
            Assert.InRange(claimDb.CreatedAt, antes, despues);

            // Exactamente 1 fila de historial inicial (null -> Open)
            Assert.Single(claimDb.StatusHistory);
            var history = claimDb.StatusHistory.First();
            Assert.Null(history.PreviousStatus);
            Assert.Equal(ClaimStatus.Open, history.NewStatus);
            Assert.Equal(userId, history.ChangedBy);
            Assert.Equal(claimDb.CreatedAt, history.ChangedAt);
        }
    }

    [Fact]
    public async Task ChangeStatusAsync_TransicionValida_ActualizaEstadoAuditoriaYAgregaHistorial()
    {
        // Arrange: Siniestro 1 del seed está en estado Open
        using var testDb = new TestDb();
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var userId = "777";
        var request = new ChangeStatusRequest
        {
            NewStatus = ClaimStatus.InReview,
            Reason = "   En revisión técnica por ajustador   "
        };

        // Act
        ClaimDetailResponse updated;
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            updated = await service.ChangeStatusAsync(1, request, userId);
        }
        var despues = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.Equal(ClaimStatus.InReview, updated.CurrentStatus);
        Assert.Equal(userId, updated.UpdatedBy);
        Assert.NotNull(updated.UpdatedAt);

        using (var assertContext = testDb.CreateDbContext())
        {
            var claimDb = await assertContext.Claims
                .Include(c => c.StatusHistory)
                .FirstAsync(c => c.Id == 1);

            Assert.Equal(ClaimStatus.InReview, claimDb.CurrentStatus);
            Assert.Equal(userId, claimDb.UpdatedBy);
            Assert.NotNull(claimDb.UpdatedAt);
            Assert.InRange(claimDb.UpdatedAt.Value, antes, despues);

            // El seed traía 1 historial, ahora debe tener 2
            Assert.Equal(2, claimDb.StatusHistory.Count);

            var nuevoHistorial = claimDb.StatusHistory.OrderByDescending(h => h.Id).First();
            Assert.Equal(ClaimStatus.Open, nuevoHistorial.PreviousStatus);
            Assert.Equal(ClaimStatus.InReview, nuevoHistorial.NewStatus);
            Assert.Equal("En revisión técnica por ajustador", nuevoHistorial.ChangeReason);
            Assert.Equal(userId, nuevoHistorial.ChangedBy);
            Assert.Equal(claimDb.UpdatedAt.Value, nuevoHistorial.ChangedAt);
        }
    }

    [Fact]
    public async Task ChangeStatusAsync_TransicionInvalida_LanzaExcepcionYNoModificaEstadoNiHistorial()
    {
        // Arrange: Siniestro 1 está en Open. Transición directa Open -> Closed es inválida
        using var testDb = new TestDb();
        var request = new ChangeStatusRequest
        {
            NewStatus = ClaimStatus.Closed,
            Reason = "Cierre directo indebido"
        };

        // Act & Assert de excepción
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
                service.ChangeStatusAsync(1, request, "999"));
        }

        // Assert de base de datos intacta
        using (var assertContext = testDb.CreateDbContext())
        {
            var claimDb = await assertContext.Claims
                .Include(c => c.StatusHistory)
                .FirstAsync(c => c.Id == 1);

            Assert.Equal(ClaimStatus.Open, claimDb.CurrentStatus);
            Assert.Null(claimDb.UpdatedAt);
            Assert.Null(claimDb.UpdatedBy);
            Assert.Single(claimDb.StatusHistory);
        }
    }

    [Fact]
    public async Task DeleteAsync_SiniestroExistente_AplicaBorradoLogicoYNoApareceEnListadoNiDetalle()
    {
        // Arrange
        using var testDb = new TestDb();
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var userId = "888";

        // Act
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            await service.DeleteAsync(1, userId);
        }
        var despues = DateTime.UtcNow.AddSeconds(1);

        // Assert con contexto nuevo
        using (var assertContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(assertContext, NullLogger<ClaimService>.Instance);

            // GetByIdAsync debe lanzar ClaimNotFoundException
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.GetByIdAsync(1));

            // GetAllAsync no debe incluirlo
            var all = await service.GetAllAsync(null, null);
            Assert.DoesNotContain(all, c => c.Id == 1);

            // Con IgnoreQueryFilters la fila SIGUE en la base con auditoría
            var raw = await assertContext.Claims
                .IgnoreQueryFilters()
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.Id == 1);

            Assert.NotNull(raw);
            Assert.True(raw.IsDeleted);
            Assert.Equal(userId, raw.DeletedBy);
            Assert.NotNull(raw.DeletedAt);
            Assert.InRange(raw.DeletedAt.Value, antes, despues);
            Assert.NotEmpty(raw.StatusHistory);

            // Un segundo DeleteAsync lanza ClaimNotFoundException
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.DeleteAsync(1, userId));
        }
    }

    [Fact]
    public async Task DeleteAsync_DeleteFisicoSqlConHistorial_FallaPorRestriccionLlaveForanea()
    {
        // Arrange
        using var testDb = new TestDb();

        // Act & Assert: Siniestro 1 tiene filas en ClaimStatusHistories y OnDelete es Restrict
        await using var context = testDb.CreateDbContext();

        var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Claims WHERE Id = 1;");
        });

        // Verificar que el fallo sea por restricción de llave foránea (SqliteException código 19 SQLITE_CONSTRAINT)
        var esSqliteConstraint = ex is SqliteException sex && sex.SqliteErrorCode == 19
            || ex.InnerException is SqliteException innerSex && innerSex.SqliteErrorCode == 19
            || ex.Message.Contains("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase);

        Assert.True(esSqliteConstraint, $"Se esperaba violación de llave foránea pero se obtuvo: {ex.Message}");
    }

    [Theory]
    [InlineData("pol-2024", 3)]
    [InlineData("001", 1)]
    [InlineData("%", 0)]
    [InlineData("_", 0)]
    [InlineData("\\", 0)]
    [InlineData("   ", 3)]
    public async Task GetAllAsync_BusquedaPorPoliza_AplicaFiltroYEscapaComodines(string filtroPoliza, int esperados)
    {
        // Arrange
        using var testDb = new TestDb();
        await using var context = testDb.CreateDbContext();
        var service = new ClaimService(context, NullLogger<ClaimService>.Instance);

        // Act
        var resultados = await service.GetAllAsync(null, filtroPoliza);

        // Assert
        Assert.Equal(esperados, resultados.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltroPorEstado_DevuelveSoloSiniestrosEnEseEstado()
    {
        // Arrange
        using var testDb = new TestDb();
        await using var context = testDb.CreateDbContext();
        var service = new ClaimService(context, NullLogger<ClaimService>.Instance);

        // Act
        var resultados = await service.GetAllAsync(ClaimStatus.Open, null);

        // Assert
        Assert.NotEmpty(resultados);
        Assert.All(resultados, c => Assert.Equal(ClaimStatus.Open, c.CurrentStatus));
    }

    [Fact]
    public async Task GetAllAsync_Ordenamiento_OrdenaPorCreatedAtDescendenteYIdDescendente()
    {
        // Arrange: Insertamos registros con fechas específicas para verificar el orden
        using var testDb = new TestDb();
        var baseDate = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

        using (var setupContext = testDb.CreateDbContext())
        {
            // Insertar siniestro A con fecha más reciente
            setupContext.Claims.Add(new Claim
            {
                PolicyNumber = "POL-ORD-A",
                InsuredName = "Insured A",
                ClaimType = "Tipo A",
                EstimatedAmount = 100m,
                CurrentStatus = ClaimStatus.Open,
                CreatedAt = baseDate.AddDays(10),
                CreatedBy = "user1"
            });

            // Insertar siniestros B y C con exactamente la MISMA fecha para probar el desempate por Id descendente
            setupContext.Claims.Add(new Claim
            {
                PolicyNumber = "POL-ORD-B",
                InsuredName = "Insured B",
                ClaimType = "Tipo B",
                EstimatedAmount = 200m,
                CurrentStatus = ClaimStatus.Open,
                CreatedAt = baseDate,
                CreatedBy = "user1"
            });

            setupContext.Claims.Add(new Claim
            {
                PolicyNumber = "POL-ORD-C",
                InsuredName = "Insured C",
                ClaimType = "Tipo C",
                EstimatedAmount = 300m,
                CurrentStatus = ClaimStatus.Open,
                CreatedAt = baseDate,
                CreatedBy = "user1"
            });

            await setupContext.SaveChangesAsync();
        }

        // Act
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            var list = await service.GetAllAsync(null, null);

            // Assert: Comprobar orden CreatedAt DESC, Id DESC
            for (var i = 0; i < list.Count - 1; i++)
            {
                var actual = list[i];
                var siguiente = list[i + 1];

                Assert.True(
                    actual.CreatedAt > siguiente.CreatedAt ||
                    (actual.CreatedAt == siguiente.CreatedAt && actual.Id > siguiente.Id),
                    $"El elemento en posición {i} (CreatedAt: {actual.CreatedAt:O}, Id: {actual.Id}) debe ser mayor o igual que posición {i+1} (CreatedAt: {siguiente.CreatedAt:O}, Id: {siguiente.Id})"
                );
            }
        }
    }

    [Fact]
    public async Task UpdateAsync_DatosValidos_ActualizaCamposYUpdatedBySinModificarEstadoNiHistorial()
    {
        // Arrange
        using var testDb = new TestDb();
        var userId = "555";
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var updateRequest = new UpdateClaimRequest
        {
            PolicyNumber = "POL-2024-MODIFICADA",
            InsuredName = "Nombre Actualizado",
            ClaimType = "Daño Parcial",
            EstimatedAmount = 8888.88m
        };

        // Act
        using (var actContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(actContext, NullLogger<ClaimService>.Instance);
            await service.UpdateAsync(1, updateRequest, userId);
        }
        var despues = DateTime.UtcNow.AddSeconds(1);

        // Assert
        using (var assertContext = testDb.CreateDbContext())
        {
            var claimDb = await assertContext.Claims
                .Include(c => c.StatusHistory)
                .FirstAsync(c => c.Id == 1);

            Assert.Equal("POL-2024-MODIFICADA", claimDb.PolicyNumber);
            Assert.Equal("Nombre Actualizado", claimDb.InsuredName);
            Assert.Equal("Daño Parcial", claimDb.ClaimType);
            Assert.Equal(8888.88m, claimDb.EstimatedAmount);
            Assert.Equal(userId, claimDb.UpdatedBy);
            Assert.NotNull(claimDb.UpdatedAt);
            Assert.InRange(claimDb.UpdatedAt.Value, antes, despues);

            // El estado debe permanecer intacto y el historial NO debe haber crecido
            Assert.Equal(ClaimStatus.Open, claimDb.CurrentStatus);
            Assert.Single(claimDb.StatusHistory);
        }
    }

    [Fact]
    public async Task UpdateAsync_Y_ChangeStatusAsync_IdInexistenteOBorrado_LanzanClaimNotFoundException()
    {
        // Arrange
        using var testDb = new TestDb();
        var updateRequest = new UpdateClaimRequest
        {
            PolicyNumber = "POL-TEST",
            InsuredName = "Test",
            ClaimType = "Test",
            EstimatedAmount = 100m
        };
        var statusRequest = new ChangeStatusRequest
        {
            NewStatus = ClaimStatus.InReview
        };

        // Borrar lógicamente el siniestro 2
        using (var deleteContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(deleteContext, NullLogger<ClaimService>.Instance);
            await service.DeleteAsync(2, "admin");
        }

        using (var testContext = testDb.CreateDbContext())
        {
            var service = new ClaimService(testContext, NullLogger<ClaimService>.Instance);

            // Id inexistente (999)
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.UpdateAsync(999, updateRequest, "user"));
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.ChangeStatusAsync(999, statusRequest, "user"));

            // Id borrado lógicamente (2)
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.UpdateAsync(2, updateRequest, "user"));
            await Assert.ThrowsAsync<ClaimNotFoundException>(() => service.ChangeStatusAsync(2, statusRequest, "user"));
        }
    }

    [Fact]
    public async Task DateTime_FechasEnBaseDeDatos_SeRecuperanConKindUtcYLocalSeGuardaComoUtc()
    {
        // Arrange
        using var testDb = new TestDb();
        var localDateTime = new DateTime(2026, 6, 15, 14, 30, 0, DateTimeKind.Local);
        var expectedUtc = localDateTime.ToUniversalTime();

        int insertedId;
        using (var insertContext = testDb.CreateDbContext())
        {
            var claim = new Claim
            {
                PolicyNumber = "POL-LOCAL-TEST",
                InsuredName = "Test Fechas",
                ClaimType = "Prueba UTC",
                EstimatedAmount = 123.45m,
                CurrentStatus = ClaimStatus.Open,
                CreatedAt = localDateTime, // Kind = Local
                CreatedBy = "userTest"
            };

            insertContext.Claims.Add(claim);
            await insertContext.SaveChangesAsync();
            insertedId = claim.Id;
        }

        // Act & Assert con contexto nuevo
        using (var readContext = testDb.CreateDbContext())
        {
            var claimDb = await readContext.Claims.FirstAsync(c => c.Id == insertedId);

            // Se recupera con Kind = Utc
            Assert.Equal(DateTimeKind.Utc, claimDb.CreatedAt.Kind);

            // La fecha guardada coincide con la conversión UTC de la fecha Local original
            Assert.Equal(expectedUtc, claimDb.CreatedAt);

            // Las fechas de los siniestros del seed también deben ser UTC
            var seedClaim = await readContext.Claims.FirstAsync(c => c.Id == 1);
            Assert.Equal(DateTimeKind.Utc, seedClaim.CreatedAt.Kind);
        }
    }
}
