namespace ZurichClaims.Api.Models.Entities;

public class ClaimStatusHistory
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    // Navigation property
    public Claim Claim { get; set; } = null!;

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string? ChangeReason { get; set; }

    public DateTime ChangedAt { get; set; }
}

