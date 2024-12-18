using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Auth.DataAccess;
using Auth.Domain.Model;
using Auth.Domain.Service;
using Auth.Helpers;
using Auth.Service;
using Common.Auth;
using Common.Cache;
using Common.Configuration;
using Common.Constants;
using Common.Encryption;
using Common.SecretManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Auth.Tests")]


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(builder.Configuration["Database:ConnectionString"].TrimStart('"').TrimEnd('"')));


builder.Services.AddLogging(builder =>
    {
      builder.AddConsole(); // Log to the console
      builder.AddDebug();   // Log debug messages
    });

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {

                      policy
                       .WithOrigins(JsonConvert.DeserializeObject<string[]>(builder.Configuration["CorsWhiteList"]))
                       .AllowCredentials()
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .SetIsOriginAllowed(hostName => true);
                    });
});

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ISecretManagementService>(serviceProvider =>
       {
         var configuration = serviceProvider.GetRequiredService<IConfiguration>();
         var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

         return environment == "Production"
              ? new AzureSecretManagementService(configuration)
              : new ConfigurationSecretManagementService(configuration);
       });

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<ICommonAuthService, CommonAuthService>();
builder.Services.AddTransient<IAuthService, AuthService>();

// For Identity  
builder.Services.AddIdentity<User, Role>(x =>
{
  x.User.RequireUniqueEmail = true;
  x.SignIn.RequireConfirmedEmail = true;
  x.SignIn.RequireConfirmedAccount = true;
  x.SignIn.RequireConfirmedPhoneNumber = false;
})
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
  options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(GeneralConstant.AuthCookieName, options =>
{
  options.Cookie.Name = GeneralConstant.AuthCookieName;
  options.LoginPath = "api/v1/login";
  options.LogoutPath = "api/v1/logout";
}).AddJwtBearer(options =>
{
  var secretService = builder.Services.BuildServiceProvider().GetRequiredService<ISecretManagementService>();

  options.SaveToken = true;
  options.RequireHttpsMetadata = false;
  options.TokenValidationParameters = new TokenValidationParameters()
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWTKey:ValidAudience"],
    ValidIssuer = builder.Configuration["JWTKey:ValidIssuer"],
    ClockSkew = TimeSpan.Zero,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretService.GetSecretAsync("JWTKey_Secret").Result)),
  };
});

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(o =>
{
  o.Events = new CookieAuthenticationEvents()
  {
    OnRedirectToLogin = (ctx) =>
    {
      if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
      {
        ctx.Response.StatusCode = 401;
      }

      return Task.CompletedTask;
    },
    OnRedirectToAccessDenied = (ctx) =>
    {
      if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
      {
        ctx.Response.StatusCode = 403;
      }

      return Task.CompletedTask;
    }
  };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Auth",
    Version = "v1",
    Description = "API documentation for Minimal API with authentication."
  });

  // Add authentication to Swagger
  c.AddSecurityDefinition("API_Key", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-API-KEY",
    Description = "Api Key authentication"
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "API_Key"
                }
            },
            Array.Empty<string>()
        }
    });
});



var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.UseMiddleware<AuthorizationMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
{
  // Gateway Swagger
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
});
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();


// Global exception handler
app.UseAuthExceptionHandler();

app.Run();

