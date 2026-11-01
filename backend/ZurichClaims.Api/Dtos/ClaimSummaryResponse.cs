using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Dtos;

public class ClaimSummaryResponse
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public ClaimStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
