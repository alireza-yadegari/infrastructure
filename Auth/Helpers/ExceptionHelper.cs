using Microsoft.AspNetCore.Diagnostics;

namespace Auth.Helpers;

internal static class ExceptionHelper
{
  internal static void UseAuthExceptionHandler(this WebApplication app)
  {
    app.UseExceptionHandler(errorApp =>
    {
      errorApp.Run(async context =>
  {
    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

    context.Response.ContentType = "application/json";

    if (exceptionHandlerPathFeature?.Error is LoginException customException)
    {
      // Handle CustomException
      context.Response.StatusCode = customException.StatusCode;
      await context.Response.WriteAsJsonAsync(new
      {
        error = customException.Message,
        statusCode = StatusCodes.Status401Unauthorized
      });
    }
    else if (exceptionHandlerPathFeature?.Error is ExternalLoginException _ ||
    exceptionHandlerPathFeature?.Error is NotFoundUserException _ ||
    exceptionHandlerPathFeature?.Error is RegisterException _)
    {
      // Handle CustomException
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      await context.Response.WriteAsJsonAsync(new
      {
        error = exceptionHandlerPathFeature?.Error.Message,
        statusCode = StatusCodes.Status400BadRequest
      });
    }
    else if (exceptionHandlerPathFeature?.Error is Exception ex)
    {
      // Handle generic exceptions
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      await context.Response.WriteAsJsonAsync(new
      {
        error = "An unexpected error occurred.",
        details = ex.Message,
        statusCode = StatusCodes.Status500InternalServerError
      });
    }
  });
    });
  }
}