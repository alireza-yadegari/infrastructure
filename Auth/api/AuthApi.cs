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

      var response = await authService.LoginAsync(loginRequest);
      var authenticationType = await configurationService.GetAuthenticationTypeAsync();

      if (response.Status == Auth.Domain.Enumeration.LoginStatus.LoggedIn)
      {
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

          httpContext.Response.Cookies.Append(GeneralConstant.AuthCookieName, response.Token, cookieOptions);
        }
        else
        {
          httpContext.Response.Headers.Authorization = new Microsoft.Extensions.Primitives.StringValues($"Bearer {response.Token}");
        }
      }
      else
      {
        httpContext.Response.Cookies.Delete(GeneralConstant.AuthCookieName);
        httpContext.Response.Headers.Authorization = "";
      }

      return response;
    })
    .WithTags("Authentication")
    .WithSummary("Login")
    .WithOpenApi();

    app.MapPost("api/v1/enable-2fa", async (
         [FromServices] IAuthService authService,
          [FromServices] IConfigurationService configurationService,
         HttpContext httpContext) =>
     {
       await authService.EnableTwoFactorAsync(await httpContext.Request.ToUserId(authService, configurationService));
     })
    .WithTags("Authentication")
    .WithSummary("Enable two factor authentication")
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
