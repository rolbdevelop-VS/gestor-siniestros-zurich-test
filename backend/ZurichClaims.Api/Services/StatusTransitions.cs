using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Services;

internal static class StatusTransitions
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> Allowed = new()
    {
        [ClaimStatus.Open] = [ClaimStatus.InReview],
        [ClaimStatus.InReview] = [ClaimStatus.Open, ClaimStatus.Closed],
        [ClaimStatus.Closed] = [ClaimStatus.InReview],
    };

    public static bool IsValid(ClaimStatus from, ClaimStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}

