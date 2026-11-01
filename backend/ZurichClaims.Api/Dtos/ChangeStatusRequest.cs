using System.ComponentModel.DataAnnotations;
using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Dtos;

public class ChangeStatusRequest
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    [EnumDataType(typeof(ClaimStatus), ErrorMessage = "El estado proporcionado no es válido.")]
    public ClaimStatus? NewStatus { get; set; }

    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres.")]
    public string? Reason { get; set; }
}
