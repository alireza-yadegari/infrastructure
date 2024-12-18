using Common.Auth;
using Common.Configuration;
using Common.Constants;
using Common.Encryption;

namespace Auth.Middlewares;

internal class AuthorizationMiddleware
{
  private readonly RequestDelegate _next;
  public AuthorizationMiddleware(
    RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(
    HttpContext context,
    IConfigurationService configurationService,
     ICommonAuthService authService)
  {
    var publicUrls = await configurationService.GetPublicUrlsAsync();
    var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value.ToLower() : "/ThisIsRoot";

    if (publicUrls.Any(x => requestPath.ToLower().StartsWith(x.ToLower())))
    {
      await _next(context);
      return;
    }
    
    string? encryptedToken = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

    if (encryptedToken != null)
    {
      try
      {
        var jwt = await configurationService.GetJWTKeyAsync();
        var validationResult = await authService.ValidateAsync(new Common.Auth.Dto.AuthValidateRequest(false, encryptedToken, "", jwt.ValidAudience, jwt.ValidIssuer, jwt.Secret));

        if (validationResult.IsValid)
        {
          await _next(context);
          return;
        }
      }
      catch
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync("User is not Logged in (Auth).");
      }

    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsJsonAsync("User is not Logged in (Auth).");
  }
}