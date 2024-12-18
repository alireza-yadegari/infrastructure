using Common.Auth;
using Common.Configuration;
using Common.Constants;
using Common.Encryption;

namespace Common.Middlewares;

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
     ICommonAuthService authService,
    IEncryptionService encryptionService)
  {

    var publicUrls = await configurationService.GetPublicUrlsAsync();
    var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value.ToLower() : "/ThisIsRoot";

    if (publicUrls.Any(x => requestPath.ToLower().StartsWith(x.ToLower())))
    {
      await _next(context);
      return; 
    }

    var authType = await configurationService.GetAuthenticationTypeAsync();
    string? encryptedToken = null;

    if (authType == "Cookie")
      encryptedToken = context.Request.Cookies?.FirstOrDefault(x => x.Key.Equals(GeneralConstant.AuthCookieName, StringComparison.InvariantCultureIgnoreCase)).Value;
    else
    {
      encryptedToken = context.Request.Headers.Authorization;
      encryptedToken = encryptedToken?.Replace("Bearer ", "");
    }


    if (encryptedToken != null)
    {
      try
      {
        var passPhrase = await configurationService.GetAuthenticationPassPhraseAsync();
        var jwt = await configurationService.GetJWTKeyAsync();
        var validationResult = await authService.ValidateAsync(new Common.Auth.Dto.AuthValidateRequest(true, encryptedToken, passPhrase, jwt.ValidAudience, jwt.ValidIssuer, jwt.Secret));

        if (validationResult.IsValid)
        {
          context.Request.Headers.Authorization = $"Bearer ${await encryptionService.DecryptAsync(encryptedToken, passPhrase)}";

          await _next(context);
          return;
        }
      }
      catch
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync("User is not Logged in.");
      }

    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsJsonAsync("User is not Logged in.");
  }
}