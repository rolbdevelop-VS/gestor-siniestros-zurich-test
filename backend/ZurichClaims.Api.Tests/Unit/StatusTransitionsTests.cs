using Xunit;
using ZurichClaims.Api.Models.Entities;
using ZurichClaims.Api.Services;

namespace ZurichClaims.Api.Tests.Unit;

public class StatusTransitionsTests
{
    [Theory]
    [InlineData(ClaimStatus.Open, ClaimStatus.InReview)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.Open)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.Closed)]
    [InlineData(ClaimStatus.Closed, ClaimStatus.InReview)]
    public void IsValid_TransicionesPermitidas_RetornaTrue(ClaimStatus origen, ClaimStatus destino)
    {
        // Act
        var esValida = StatusTransitions.IsValid(origen, destino);

        // Assert
        Assert.True(esValida);
    }

    [Theory]
    [InlineData(ClaimStatus.Open, ClaimStatus.Closed)]
    [InlineData(ClaimStatus.Closed, ClaimStatus.Open)]
    [InlineData(ClaimStatus.Open, ClaimStatus.Open)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.InReview)]
    [InlineData(ClaimStatus.Closed, ClaimStatus.Closed)]
    public void IsValid_TransicionesInvalidas_RetornaFalse(ClaimStatus origen, ClaimStatus destino)
    {
        // Act
        var esValida = StatusTransitions.IsValid(origen, destino);

        // Assert
        Assert.False(esValida);
    }
}

