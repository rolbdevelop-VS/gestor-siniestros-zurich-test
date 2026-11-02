using System.ComponentModel.DataAnnotations;

namespace ZurichClaims.Api.Dtos;

public class CreateClaimRequest
{
    public const string MinEstimatedAmount = "0.01";
    public const string MaxEstimatedAmount = "999999999999.99";
    public const string AmountRangeErrorMessage = "El monto estimado debe ser mayor a 0 y no exceder 999,999,999,999.99.";

    [Required(ErrorMessage = "El número de póliza es obligatorio.")]
    [StringLength(50, ErrorMessage = "El número de póliza no puede exceder 50 caracteres.")]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del asegurado es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre del asegurado no puede exceder 150 caracteres.")]
    public string InsuredName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de siniestro es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo de siniestro no puede exceder 100 caracteres.")]
    public string ClaimType { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto estimado es obligatorio.")]
    [Range(typeof(decimal), MinEstimatedAmount, MaxEstimatedAmount, ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = AmountRangeErrorMessage)]
    [MaxDecimalPlaces(2, ErrorMessage = "El monto estimado no puede tener más de 2 decimales.")]
    public decimal? EstimatedAmount { get; set; }
}
