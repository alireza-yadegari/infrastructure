using Common.Configuration;

namespace Common.Middlewares;

internal class ApiKeyMiddleware
{
  private readonly RequestDelegate _next;
  private const string ApiKeyHeaderName = "X-API-KEY";

  public ApiKeyMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, IConfigurationService configurationService)
  {
    var publicEndpoints = new[]
    {
      "/swagger",       // Base Swagger path
      "/swagger/index.html",
      "/swagger/v1/swagger.json"
    };

    if (publicEndpoints.Any(endpoint => context.Request.Path.StartsWithSegments(endpoint, StringComparison.OrdinalIgnoreCase)))
    {
      await _next(context);
      return;
    }

    if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await context.Response.WriteAsJsonAsync("API Key was not provided.");
      return;
    }

    var validApiKeys = await configurationService.GetClientApiKeysAsync();

    if (!validApiKeys?.Contains(extractedApiKey) ?? false)
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      await context.Response.WriteAsJsonAsync("Invalid API Key.");
      return; 
    }

    await _next(context);
  }
}