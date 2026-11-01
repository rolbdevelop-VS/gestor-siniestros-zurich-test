using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ZurichClaims.Api.Exceptions;

namespace ZurichClaims.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ClaimNotFoundException notFoundEx:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Siniestro no encontrado";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5";
                problemDetails.Detail = notFoundEx.Message;
                break;

            case InvalidStatusTransitionException transitionEx:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Transición de estado no permitida";
                problemDetails.Type = "https://api.zurich.claims/errors/invalid-status-transition";
                problemDetails.Detail = transitionEx.Message;
                break;

            default:
                _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Error interno del servidor";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";
                problemDetails.Detail = "Ha ocurrido un error inesperado al procesar la solicitud.";
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
