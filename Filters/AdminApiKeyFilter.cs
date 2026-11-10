using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters;

public sealed class AdminApiKeyFilter(IConfiguration configuration, ILogger<AdminApiKeyFilter> logger)
    : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var expected = configuration["Seguridad:ClaveAdministrador"];
        var received = context.HttpContext.Request.Headers["X-Admin-Key"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(expected ?? string.Empty);
        var receivedBytes = Encoding.UTF8.GetBytes(received);

        if (expectedBytes.Length == 0 || receivedBytes.Length == 0 ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes))
        {
            logger.LogWarning("Intento no autorizado de reiniciar la experiencia de un personaje.");
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Se requiere una clave de administrador válida."
            });
        }

        return Task.CompletedTask;
    }
}
