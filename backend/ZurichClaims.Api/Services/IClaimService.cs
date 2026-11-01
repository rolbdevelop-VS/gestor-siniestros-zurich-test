using ZurichClaims.Api.Dtos;
using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Services;

public interface IClaimService
{
    Task<ClaimDetailResponse> CreateAsync(CreateClaimRequest request, string userId, CancellationToken ct = default);
    Task<IReadOnlyList<ClaimSummaryResponse>> GetAllAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct = default);
    Task<ClaimDetailResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClaimSummaryResponse> UpdateAsync(int id, UpdateClaimRequest request, string userId, CancellationToken ct = default);
    Task DeleteAsync(int id, string userId, CancellationToken ct = default);
}

