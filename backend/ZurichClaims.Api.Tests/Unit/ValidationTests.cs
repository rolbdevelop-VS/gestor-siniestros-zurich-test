using System.ComponentModel.DataAnnotations;
using Xunit;
using ZurichClaims.Api.Dtos;

namespace ZurichClaims.Api.Tests.Unit;

public class ValidationTests
{
    private static (bool IsValid, List<ValidationResult> Results) Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return (isValid, results);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("999999999999.99")]
    public void CreateClaimRequest_MontosValidos_PasanValidacion(string montoString)
    {
        // Arrange
        var monto = decimal.Parse(montoString, System.Globalization.CultureInfo.InvariantCulture);
        var request = new CreateClaimRequest
        {
            PolicyNumber = "POL-2024-001",
            InsuredName = "Juan Pérez",
            ClaimType = "Colisión",
            EstimatedAmount = monto
        };

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        Assert.True(isValid, $"Se esperaba válido pero falló con: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
        Assert.Empty(results);
    }

    [Fact]
    public void CreateClaimRequest_MontoNulo_FallaValidacion()
    {
        // Arrange
        var request = new CreateClaimRequest
        {
            PolicyNumber = "POL-2024-001",
            InsuredName = "Juan Pérez",
            ClaimType = "Colisión",
            EstimatedAmount = null
        };

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.999")]
    [InlineData("1000000000000")]
    [InlineData("79228162514264337593543950335")]
    public void CreateClaimRequest_MontosInvalidos_FallanValidacion(string montoString)
    {
        // Arrange
        var monto = decimal.Parse(montoString, System.Globalization.CultureInfo.InvariantCulture);
        var request = new CreateClaimRequest
        {
            PolicyNumber = "POL-2024-001",
            InsuredName = "Juan Pérez",
            ClaimType = "Colisión",
            EstimatedAmount = monto
        };

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateClaimRequest_CamposTextoVaciosOSoloEspacios_FallanValidacion(string valorInvalido)
    {
        // Arrange & Act & Assert para PolicyNumber
        var req1 = new CreateClaimRequest { PolicyNumber = valorInvalido, InsuredName = "Juan", ClaimType = "Robo", EstimatedAmount = 100m };
        var (v1, r1) = Validate(req1);
        Assert.False(v1);
        Assert.Contains(r1, r => r.MemberNames.Contains(nameof(CreateClaimRequest.PolicyNumber)));

        // Arrange & Act & Assert para InsuredName
        var req2 = new CreateClaimRequest { PolicyNumber = "POL-001", InsuredName = valorInvalido, ClaimType = "Robo", EstimatedAmount = 100m };
        var (v2, r2) = Validate(req2);
        Assert.False(v2);
        Assert.Contains(r2, r => r.MemberNames.Contains(nameof(CreateClaimRequest.InsuredName)));

        // Arrange & Act & Assert para ClaimType
        var req3 = new CreateClaimRequest { PolicyNumber = "POL-001", InsuredName = "Juan", ClaimType = valorInvalido, EstimatedAmount = 100m };
        var (v3, r3) = Validate(req3);
        Assert.False(v3);
        Assert.Contains(r3, r => r.MemberNames.Contains(nameof(CreateClaimRequest.ClaimType)));
    }

    [Fact]
    public void CreateClaimRequest_CamposTextoExcedenLongitudMaxima_FallanValidacion()
    {
        // PolicyNumber > 50
        var req1 = new CreateClaimRequest { PolicyNumber = new string('A', 51), InsuredName = "Juan", ClaimType = "Robo", EstimatedAmount = 100m };
        var (v1, r1) = Validate(req1);
        Assert.False(v1);
        Assert.Contains(r1, r => r.MemberNames.Contains(nameof(CreateClaimRequest.PolicyNumber)));

        // InsuredName > 150
        var req2 = new CreateClaimRequest { PolicyNumber = "POL-001", InsuredName = new string('B', 151), ClaimType = "Robo", EstimatedAmount = 100m };
        var (v2, r2) = Validate(req2);
        Assert.False(v2);
        Assert.Contains(r2, r => r.MemberNames.Contains(nameof(CreateClaimRequest.InsuredName)));

        // ClaimType > 100
        var req3 = new CreateClaimRequest { PolicyNumber = "POL-001", InsuredName = "Juan", ClaimType = new string('C', 101), EstimatedAmount = 100m };
        var (v3, r3) = Validate(req3);
        Assert.False(v3);
        Assert.Contains(r3, r => r.MemberNames.Contains(nameof(CreateClaimRequest.ClaimType)));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("999999999999.99")]
    public void UpdateClaimRequest_MontosValidos_PasanValidacion(string montoString)
    {
        // Arrange
        var monto = decimal.Parse(montoString, System.Globalization.CultureInfo.InvariantCulture);
        var request = new UpdateClaimRequest
        {
            PolicyNumber = "POL-2024-001",
            InsuredName = "Juan Pérez",
            ClaimType = "Colisión",
            EstimatedAmount = monto
        };

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.999")]
    [InlineData("1000000000000")]
    [InlineData("79228162514264337593543950335")]
    public void UpdateClaimRequest_MontosInvalidos_FallanValidacion(string montoString)
    {
        // Arrange
        var monto = decimal.Parse(montoString, System.Globalization.CultureInfo.InvariantCulture);
        var request = new UpdateClaimRequest
        {
            PolicyNumber = "POL-2024-001",
            InsuredName = "Juan Pérez",
            ClaimType = "Colisión",
            EstimatedAmount = monto
        };

        // Act
        var (isValid, results) = Validate(request);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateClaimRequest_CamposTextoVaciosOSoloEspacios_FallanValidacion(string valorInvalido)
    {
        var req1 = new UpdateClaimRequest { PolicyNumber = valorInvalido, InsuredName = "Juan", ClaimType = "Robo", EstimatedAmount = 100m };
        var (v1, r1) = Validate(req1);
        Assert.False(v1);
        Assert.Contains(r1, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.PolicyNumber)));

        var req2 = new UpdateClaimRequest { PolicyNumber = "POL-001", InsuredName = valorInvalido, ClaimType = "Robo", EstimatedAmount = 100m };
        var (v2, r2) = Validate(req2);
        Assert.False(v2);
        Assert.Contains(r2, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.InsuredName)));

        var req3 = new UpdateClaimRequest { PolicyNumber = "POL-001", InsuredName = "Juan", ClaimType = valorInvalido, EstimatedAmount = 100m };
        var (v3, r3) = Validate(req3);
        Assert.False(v3);
        Assert.Contains(r3, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.ClaimType)));
    }

    [Fact]
    public void UpdateClaimRequest_CamposTextoExcedenLongitudMaxima_FallanValidacion()
    {
        var req1 = new UpdateClaimRequest { PolicyNumber = new string('A', 51), InsuredName = "Juan", ClaimType = "Robo", EstimatedAmount = 100m };
        var (v1, r1) = Validate(req1);
        Assert.False(v1);
        Assert.Contains(r1, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.PolicyNumber)));

        var req2 = new UpdateClaimRequest { PolicyNumber = "POL-001", InsuredName = new string('B', 151), ClaimType = "Robo", EstimatedAmount = 100m };
        var (v2, r2) = Validate(req2);
        Assert.False(v2);
        Assert.Contains(r2, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.InsuredName)));

        var req3 = new UpdateClaimRequest { PolicyNumber = "POL-001", InsuredName = "Juan", ClaimType = new string('C', 101), EstimatedAmount = 100m };
        var (v3, r3) = Validate(req3);
        Assert.False(v3);
        Assert.Contains(r3, r => r.MemberNames.Contains(nameof(UpdateClaimRequest.ClaimType)));
    }
}
