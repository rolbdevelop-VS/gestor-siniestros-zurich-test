namespace ZurichClaims.Api.Models.Entities;

public class ClaimStatusHistory
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    // Navigation property
    public Claim Claim { get; set; } = null!;

    public ClaimStatus? PreviousStatus { get; set; }

    public ClaimStatus NewStatus { get; set; }

    public string? ChangeReason { get; set; }

    public DateTime ChangedAt { get; set; }

    // Auditoría: quién realizó el cambio de estado
    public string ChangedBy { get; set; } = string.Empty;
}
