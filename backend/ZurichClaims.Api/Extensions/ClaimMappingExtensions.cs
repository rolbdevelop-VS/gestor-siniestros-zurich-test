using ZurichClaims.Api.Dtos;
using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Extensions;

public static class ClaimMappingExtensions
{
    public static ClaimSummaryResponse ToSummaryResponse(this Claim claim)
    {
        return new ClaimSummaryResponse
        {
            Id = claim.Id,
            PolicyNumber = claim.PolicyNumber,
            InsuredName = claim.InsuredName,
            ClaimType = claim.ClaimType,
            EstimatedAmount = claim.EstimatedAmount,
            CurrentStatus = claim.CurrentStatus,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt,
            CreatedBy = claim.CreatedBy,
            UpdatedBy = claim.UpdatedBy
        };
    }

    public static ClaimStatusHistoryResponse ToResponse(this ClaimStatusHistory history)
    {
        return new ClaimStatusHistoryResponse
        {
            Id = history.Id,
            PreviousStatus = history.PreviousStatus,
            NewStatus = history.NewStatus,
            ChangeReason = history.ChangeReason,
            ChangedAt = history.ChangedAt,
            ChangedBy = history.ChangedBy
        };
    }

    public static ClaimDetailResponse ToDetailResponse(this Claim claim)
    {
        return new ClaimDetailResponse
        {
            Id = claim.Id,
            PolicyNumber = claim.PolicyNumber,
            InsuredName = claim.InsuredName,
            ClaimType = claim.ClaimType,
            EstimatedAmount = claim.EstimatedAmount,
            CurrentStatus = claim.CurrentStatus,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt,
            CreatedBy = claim.CreatedBy,
            UpdatedBy = claim.UpdatedBy,
            History = claim.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .ThenBy(h => h.Id)
                .Select(h => h.ToResponse())
                .ToList()
        };
    }
}
