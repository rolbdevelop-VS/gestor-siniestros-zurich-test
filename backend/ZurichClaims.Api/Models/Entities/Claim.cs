namespace ZurichClaims.Api.Models.Entities;

public class Claim
{
    public int Id { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public string InsuredName { get; set; } = string.Empty;

    public string ClaimType { get; set; } = string.Empty;

    public decimal EstimatedAmount { get; set; }

    public ClaimStatus CurrentStatus { get; set; } = ClaimStatus.Open;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation property for 1:N relationship
    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}

