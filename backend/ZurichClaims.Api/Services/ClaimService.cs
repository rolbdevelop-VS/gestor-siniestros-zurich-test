using Microsoft.EntityFrameworkCore;
using ZurichClaims.Api.Data;
using ZurichClaims.Api.Dtos;
using ZurichClaims.Api.Exceptions;
using ZurichClaims.Api.Extensions;
using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Services;

public class ClaimService : IClaimService
{
    private readonly ClaimsDbContext _dbContext;
    private readonly ILogger<ClaimService> _logger;

    public ClaimService(ClaimsDbContext dbContext, ILogger<ClaimService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClaimDetailResponse> CreateAsync(CreateClaimRequest request, string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var claim = new Claim
        {
            PolicyNumber = request.PolicyNumber.Trim(),
            InsuredName = request.InsuredName.Trim(),
            ClaimType = request.ClaimType.Trim(),
            EstimatedAmount = request.EstimatedAmount!.Value,
            CurrentStatus = ClaimStatus.Open,
            CreatedAt = now,
            UpdatedAt = null,
            CreatedBy = userId.Trim(),
            UpdatedBy = null,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        var initialHistory = new ClaimStatusHistory
        {
            PreviousStatus = null,
            NewStatus = ClaimStatus.Open,
            ChangeReason = "Apertura y registro inicial del siniestro",
            ChangedAt = now,
            ChangedBy = userId.Trim()
        };

        claim.StatusHistory.Add(initialHistory);

        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Siniestro {ClaimId} creado exitosamente con estado {Status} por el usuario {UserId}.", claim.Id, claim.CurrentStatus, userId);

        return claim.ToDetailResponse();
    }

    public async Task<IReadOnlyList<ClaimSummaryResponse>> GetAllAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct = default)
    {
        IQueryable<Claim> query = _dbContext.Claims.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(c => c.CurrentStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(policyNumber))
        {
            var trimmedPolicy = policyNumber.Trim();
            query = query.Where(c => EF.Functions.Like(c.PolicyNumber, $"%{trimmedPolicy}%"));
        }

        query = query.OrderByDescending(c => c.CreatedAt)
                     .ThenByDescending(c => c.Id);

        var claims = await query.ToListAsync(ct);

        return claims.Select(c => c.ToSummaryResponse()).ToList();
    }

    public async Task<ClaimDetailResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var claim = await _dbContext.Claims
            .AsNoTracking()
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (claim is null)
        {
            throw new ClaimNotFoundException(id);
        }

        return claim.ToDetailResponse();
    }

    public async Task<ClaimSummaryResponse> UpdateAsync(int id, UpdateClaimRequest request, string userId, CancellationToken ct = default)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (claim is null)
        {
            throw new ClaimNotFoundException(id);
        }

        claim.PolicyNumber = request.PolicyNumber.Trim();
        claim.InsuredName = request.InsuredName.Trim();
        claim.ClaimType = request.ClaimType.Trim();
        claim.EstimatedAmount = request.EstimatedAmount!.Value;
        claim.UpdatedAt = DateTime.UtcNow;
        claim.UpdatedBy = userId.Trim();

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Siniestro {ClaimId} actualizado por el usuario {UserId}.", claim.Id, userId);

        return claim.ToSummaryResponse();
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken ct = default)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (claim is null)
        {
            throw new ClaimNotFoundException(id);
        }

        claim.IsDeleted = true;
        claim.DeletedAt = DateTime.UtcNow;
        claim.DeletedBy = userId.Trim();

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Siniestro {ClaimId} borrado lógicamente por el usuario {UserId}.", claim.Id, userId);
    }
}

