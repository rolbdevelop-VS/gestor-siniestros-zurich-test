using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Xunit;
using ZurichClaims.Api.Dtos;

namespace ZurichClaims.Api.Tests.Unit;

public class CultureValidationTests
{
    [Theory]
    [InlineData("es-ES")]
    [InlineData("de-DE")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void ValidacionMonto_BajoDiferentesCulturas_RespetaLogicaDeNegocio(string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var targetCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = targetCulture;
            CultureInfo.CurrentUICulture = targetCulture;

            // Caso 1: 0.5 debe ser válido
            var validRequest = new CreateClaimRequest
            {
                PolicyNumber = "POL-2024-001",
                InsuredName = "Asegurado Test",
                ClaimType = "Colisión",
                EstimatedAmount = 0.5m
            };
            var validResults = new List<ValidationResult>();
            var isValid1 = Validator.TryValidateObject(validRequest, new ValidationContext(validRequest), validResults, validateAllProperties: true);
            Assert.True(isValid1, $"El monto 0.5 debió ser válido bajo cultura {cultureName}, errores: {string.Join(", ", validResults.Select(r => r.ErrorMessage))}");

            // Caso 2: 1000000000000 (1 billón) debe ser inválido
            var invalidRequest = new CreateClaimRequest
            {
                PolicyNumber = "POL-2024-001",
                InsuredName = "Asegurado Test",
                ClaimType = "Colisión",
                EstimatedAmount = 1000000000000m
            };
            var invalidResults = new List<ValidationResult>();
            var isValid2 = Validator.TryValidateObject(invalidRequest, new ValidationContext(invalidRequest), invalidResults, validateAllProperties: true);
            Assert.False(isValid2, $"El monto 1000000000000 debió ser inválido bajo cultura {cultureName}");
            Assert.Contains(invalidResults, r => r.MemberNames.Contains(nameof(CreateClaimRequest.EstimatedAmount)));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}

