using ZurichClaims.Api.Models.Entities;

namespace ZurichClaims.Api.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public ClaimStatus CurrentStatus { get; }
    public ClaimStatus RequestedStatus { get; }

    public InvalidStatusTransitionException(ClaimStatus currentStatus, ClaimStatus requestedStatus)
        : base($"No se permite la transición de estado desde '{currentStatus}' hacia '{requestedStatus}'.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }
}
