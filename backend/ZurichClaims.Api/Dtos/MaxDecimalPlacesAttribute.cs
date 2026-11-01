using System.ComponentModel.DataAnnotations;

namespace ZurichClaims.Api.Dtos;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MaxDecimalPlacesAttribute : ValidationAttribute
{
    private readonly int _maxDecimalPlaces;

    public MaxDecimalPlacesAttribute(int maxDecimalPlaces)
        : base($"El valor no puede tener más de {maxDecimalPlaces} decimales.")
    {
        _maxDecimalPlaces = maxDecimalPlaces;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is decimal decimalValue)
        {
            // Verificación matemática: multiplicamos por 10^_maxDecimalPlaces y comprobamos que no haya residuo fraccionario
            decimal multiplier = (decimal)Math.Pow(10, _maxDecimalPlaces);
            if (decimal.Remainder(decimalValue * multiplier, 1m) != 0m)
            {
                return new ValidationResult(ErrorMessage ?? $"El valor no puede tener más de {_maxDecimalPlaces} decimales.");
            }
        }

        return ValidationResult.Success;
    }
}
