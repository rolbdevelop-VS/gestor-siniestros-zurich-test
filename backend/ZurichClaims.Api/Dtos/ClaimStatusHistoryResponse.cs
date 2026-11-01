using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Dtos;

public class ClaimStatusHistoryResponse
{
    public int Id { get; set; }
    public ClaimStatus? PreviousStatus { get; set; }
    public ClaimStatus NewStatus { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
