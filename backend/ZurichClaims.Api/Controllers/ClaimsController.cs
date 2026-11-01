using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ZurichClaims.Api.Dtos;
using ZurichClaims.Api.Models.Entities;
using ZurichClaims.Api.Services;

namespace ZurichClaims.Api.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private static readonly Regex UserIdRegex = new(@"^\d{1,20}$", RegexOptions.Compiled);
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ClaimSummaryResponse>>> GetAll(
        [FromQuery] ClaimStatus? status,
        [FromQuery] string? policyNumber,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        var claims = await _claimService.GetAllAsync(status, policyNumber, ct);
        return Ok(claims);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClaimDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClaimDetailResponse>> GetById(
        [FromRoute] int id,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        var claim = await _claimService.GetByIdAsync(id, ct);
        return Ok(claim);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClaimDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClaimDetailResponse>> Create(
        [FromBody] CreateClaimRequest request,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        var created = await _claimService.CreateAsync(request, userId!, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClaimSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClaimSummaryResponse>> Update(
        [FromRoute] int id,
        [FromBody] UpdateClaimRequest request,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        var updated = await _claimService.UpdateAsync(id, request, userId!, ct);
        return Ok(updated);
    }

    [HttpPost("{id:int}/status")]
    [ProducesResponseType(typeof(ClaimDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClaimDetailResponse>> ChangeStatus(
        [FromRoute] int id,
        [FromBody] ChangeStatusRequest request,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _claimService.ChangeStatusAsync(id, request, userId!, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromHeader(Name = "x-usuario")] string? userId,
        CancellationToken ct)
    {
        if (!TryValidateUserId(userId, out var errorResult))
        {
            return errorResult!;
        }

        await _claimService.DeleteAsync(id, userId!, ct);
        return NoContent();
    }

    private static bool TryValidateUserId(string? userId, out ActionResult? errorResult)
    {
        if (string.IsNullOrWhiteSpace(userId) || !UserIdRegex.IsMatch(userId))
        {
            errorResult = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Encabezado x-usuario inválido",
                Detail = "El encabezado 'x-usuario' es requerido y debe ser un identificador numérico de hasta 20 dígitos.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            });
            return false;
        }

        errorResult = null;
        return true;
    }
}

