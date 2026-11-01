namespace ZurichClaims.Api.Exceptions;

public class ClaimNotFoundException : Exception
{
    public int ClaimId { get; }

    public ClaimNotFoundException(int claimId)
        : base($"No se encontró ningún siniestro activo con el ID '{claimId}'.")
    {
        ClaimId = claimId;
    }
}
