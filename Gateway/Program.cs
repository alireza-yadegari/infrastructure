// using Gateway.Middlewares;
using Common.Auth;
using Common.Cache;
using Common.Configuration;
using Common.Encryption;
using Common.Middlewares;
using Common.SecretManagement;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using Gateway.Helpers;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddLogging(builder =>
    {
      builder.AddConsole(); // Log to the console
      builder.AddDebug();   // Log debug messages
    });



builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ISecretManagementService>(serviceProvider =>
       {
         var configuration = serviceProvider.GetRequiredService<IConfiguration>();
         var environment = configuration["SECRET_ENVIRONMENT"] ?? "Development";

         return environment == "Production"
              ? new AzureSecretManagementService(configuration)
              : new ConfigurationSecretManagementService(configuration);
       });

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<ICommonAuthService, CommonAuthService>();
builder.Services.AddTransient<ICommonAuthService, CommonAuthService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Gateway",
    Version = "v1",
    Description = "API documentation for Gateway."
  });

  // Add authentication to Swagger
  options.AddSecurityDefinition("API_Key", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-API-KEY",
    Description = "Api Key authentication"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<Common.Middlewares.AuthorizationMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  // Dynamically fetch and merge Swagger from Auth and Order services
  var openApiReader = new OpenApiStreamReader();
  var authSwagger = await openApiReader.ReadAsync(await new HttpClient().GetStreamAsync("http://localhost:5001/swagger/v1/swagger.json"), CancellationToken.None);

  var mergedSwagger = new OpenApiDocument
  {
    Info = new OpenApiInfo { Title = "Merged Gateway API", Version = "v1" },
    Paths = new OpenApiPaths(),
    Components = new OpenApiComponents(),
    SecurityRequirements = new List<OpenApiSecurityRequirement>()
  };


  mergedSwagger.SecurityRequirements.MergeSecurity(authSwagger.OpenApiDocument.SecurityRequirements);

  mergedSwagger.Paths.MergePaths(authSwagger.OpenApiDocument.Paths, "auth");
  mergedSwagger.Components.MergeComponents(authSwagger.OpenApiDocument.Components);



  var outputString = new StringWriter();
  mergedSwagger.SerializeAsV3(new OpenApiJsonWriter(outputString));

  app.MapGet("/swagger/v1/merged-swagger.json", () => outputString.ToString());

  app.UseSwagger();
  app.UseSwaggerUI(c =>
 {
   // Gateway Swagger
   c.SwaggerEndpoint("/swagger/v1/merged-swagger.json", "Gateway API V1");
 });
}

app.MapReverseProxy();

app.Run();

