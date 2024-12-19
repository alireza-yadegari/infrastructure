using System.Net.Http.Headers;
using Auth.Domain.Dto;
using Auth.Domain.Model;
using Auth.Domain.Service;
using Auth.Helpers;
using Common.Configuration;
using Common.Constants;
using Microsoft.AspNetCore.Mvc;

public static class AuthEndpoints
{
  public static void MapAuthEndpoints(this WebApplication app)
  {
    app.MapPost("api/v1/login", async (
        [FromServices] IAuthService authService,
        [FromServices] IConfigurationService configurationService,
        HttpContext httpContext,
        LoginRequest loginRequest,
        [FromQuery] string? ReturnUrl
        ) =>
    {

      var encryptedToken = await authService.LoginAsync(loginRequest);
      var authenticationType = await configurationService.GetAuthenticationTypeAsync();
      if (authenticationType == "Cookie")
      {
        var cookieOption = await configurationService.GetCookieOptionAsync();
        var cookieOptions = new CookieOptions()
        {
          Expires = cookieOption.Expires,
          HttpOnly = cookieOption.HttpOnly,
          Secure = cookieOption.Secure,
          Path = cookieOption.Path,
          SameSite = (SameSiteMode)Enum.Parse(typeof(SameSiteMode), cookieOption.SameSite, true),
        };

        if (!string.IsNullOrEmpty(cookieOption.Domain))
        {
          cookieOptions.Domain = cookieOption.Domain;
        }

        httpContext.Response.Cookies.Append(GeneralConstant.AuthCookieName, encryptedToken, cookieOptions);
      }
      else
      {
        httpContext.Response.Headers.Authorization = new Microsoft.Extensions.Primitives.StringValues($"Bearer {encryptedToken}");
      }

      return true;
    })
    .WithTags("Authentication")
    .WithOpenApi();

    app.MapPost("api/v1/external-login", async (
            string provider,
            string returnUrl,
            [FromServices] IAuthService authService,
            HttpContext httpContext) =>
        {
          var redirectUrl = $"/auth/external-login-callback?returnUrl={returnUrl}";
          var properties = await authService.ExternalLoginAsync(provider, redirectUrl);
          return new { redirectUrl, properties };
        })
   .WithTags("Authentication")
   .WithSummary("External Login")
   .WithOpenApi();

    app.MapPost("api/v1/external-login-callback", async (
            string? remoteError,
            [FromServices] IAuthService authService,
            HttpContext httpContext) =>
        {
          if (remoteError != null)
          {
            throw new ExternalLoginException(remoteError);
          }

          await authService.ExternalLoginCallbackAsync();
        })
   .WithTags("Authentication")
   .WithSummary("External Login Callback")
   .WithOpenApi();

    app.MapPost("api/v1/enable-2fa", async (
         string? remoteError,
         [FromServices] IAuthService authService,
          [FromServices] IConfigurationService configurationService,
         HttpContext httpContext) =>
     {
       await authService.EnableTwoFactorAsync(await httpContext.Request.ToUserId(authService, configurationService));
     })
    .WithTags("Authentication")
    .WithSummary("External Login Callback")
    .WithOpenApi();

    app.MapPost("api/v1/logout", async (
      [FromServices] IConfigurationService configurationService,
      HttpContext httpContext) =>
    {
      var authenticationType = await configurationService.GetAuthenticationTypeAsync();
      if (authenticationType == "Cookie")
      {
        httpContext.Response.Cookies.Delete(GeneralConstant.AuthCookieName);
      }
      else
      {
        httpContext.Response.Headers.Remove("Authorization");
      }
      return true;
    })
    .WithTags("Authentication")
    .WithSummary("Logout and clear the cookie.");

    app.MapPost("api/v1/register", async (
        [FromServices] IAuthService authService,
        HttpContext httpContext,
        RegisterRequest registerRequest) =>
    {
      var user = await authService.RegisterAsync(
              registerRequest,
          registerRequest.role ?? "User");
      if (user == null)
      {
        throw new RegisterException("There is an error");
      }
      return new UserRegisterResponse(user.Name, user.LastName, !user.EmailConfirmed);
    })
    .WithTags("Authentication")
    .WithSummary("Register New User");

    app.MapGet("api/v1/email-verification", async (
      [FromServices] IAuthService authService,
      [FromQuery] string token,
      [FromQuery] string user,
      HttpContext httpContext
      ) =>
      {
        await authService.ConfirmAccount(user, token.Replace(" ", "+"));

        return "Your Account has been confirmed!";
      })
      .WithTags("Authentication")
      .WithSummary("Confirm New User Account");

    app.MapPost("api/v1/get-userid", async (
             [FromServices] IAuthService authService,
             [FromServices] IConfigurationService configurationService,
             HttpContext httpContext) =>
         {
           return await httpContext.Request.ToUserId(authService, configurationService);
         })
    .WithTags("Authentication")
    .WithSummary("Get User Id");
  }

}
