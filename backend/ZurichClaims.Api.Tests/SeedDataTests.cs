using Microsoft.EntityFrameworkCore;
using Xunit;
using ZurichClaims.Api.Models.Entities;
using ZurichClaims.Api.Tests.Helpers;

namespace ZurichClaims.Api.Tests;

public class SeedDataTests
{
    [Fact]
    public async Task BaseDeDatos_AlAplicarMigraciones_CargaTresSiniestrosInicialesDelSeed()
    {
        // Arrange
        using var testDb = new TestDb();
        await using var context = testDb.CreateDbContext();

        // Act
        var claims = await context.Claims
            .OrderBy(c => c.Id)
            .ToListAsync();

        // Assert
        Assert.Equal(3, claims.Count);

        Assert.Equal(1, claims[0].Id);
        Assert.Equal("POL-2024-001", claims[0].PolicyNumber);
        Assert.Equal(ClaimStatus.Open, claims[0].CurrentStatus);

        Assert.Equal(2, claims[1].Id);
        Assert.Equal("POL-2024-002", claims[1].PolicyNumber);
        Assert.Equal(ClaimStatus.InReview, claims[1].CurrentStatus);

        Assert.Equal(3, claims[2].Id);
        Assert.Equal("POL-2024-003", claims[2].PolicyNumber);
        Assert.Equal(ClaimStatus.Closed, claims[2].CurrentStatus);
    }
}

