using Common.Cache;
using Common.Configuration.Dto;
using Common.SecretManagement;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Common.Configuration;
public class ConfigurationService : IConfigurationService
{
  private readonly IConfiguration configuration;
  private readonly ICacheService cacheService;

  private readonly ISecretManagementService secretManagementService;
  public ConfigurationService(
    IConfiguration configuration,
    ICacheService cacheService,
    ISecretManagementService secretManagementService)
  {
    this.configuration = configuration;
    this.cacheService = cacheService;
    this.secretManagementService = secretManagementService;
  }

  public async Task<CookieOption?> GetCookieOptionAsync()
  {
    var _TokenExpiryTimeInHour = Convert.ToInt64(configuration["JWTInformation:TokenExpiryTimeInHour"]);

    var cookieOption = new CookieOption(

      DateTime.UtcNow.AddHours(_TokenExpiryTimeInHour),
      true,
      configuration["Cookie:Path"],
      bool.Parse(configuration["Cookie:Secure"] ?? "true"),
      configuration["Cookie:SameSite"] ?? "Lax",
      configuration["Cookie:Domain"]
    );

    return cookieOption;
  }

  public async Task<AuthTwoFactorAuthentication?> GetTwoFactorAuthenticationAsync()
  {
    var rootSection = configuration.GetRequiredSection("AuthTwoFactorAuthentication");
    return new AuthTwoFactorAuthentication(rootSection.GetSection("EMailVerificationEndpoint").Value ?? "");
  }

  public async Task<JWTInformation?> GetJWTKeyAsync()
  {
    var rootSection = configuration.GetRequiredSection("JWTInformation");

    if (rootSection == null)
    {
      return null;
    }

    var result = new JWTInformation(
      rootSection.GetSection("ValidAudience").Value ?? "",
      rootSection.GetSection("ValidIssuer").Value ?? "",
      int.Parse(rootSection.GetSection("TokenExpiryTimeInHour").Value ?? "0")
    );


    var secret = await cacheService.GetAsync<string>("JWTKey_Secret");
    if (secret == null)
    {
      secret = await secretManagementService.GetSecretAsync("JWTKey_Secret");
      await cacheService.SetAsync("JWTKey_Secret", secret, 8 * 60);
    }

    result = result with { Secret = secret };
    return result;
  }

  public async Task<string> GetAuthenticationTypeAsync()
  {
    return configuration["AuthenticationType"] ?? "JWT";
  }

  public async Task<List<string>> GetClientApiKeysAsync()
  {
    var secret = await cacheService.GetAsync<string>("ApiKeys");
    if (secret == null)
    {
      secret = await secretManagementService.GetSecretAsync("ApiKeys");
      await cacheService.SetAsync("ApiKeys", secret, 8 * 60);
    }
    return JsonConvert.DeserializeObject<List<string>>(secret) ?? new List<string>();
  }

  public async Task<string> GetAuthenticationPassPhraseAsync()
  {
    var secret = await cacheService.GetAsync<string>("AuthenticationPassPhrase");
    if (secret == null)
    {
      secret = await secretManagementService.GetSecretAsync("AuthenticationPassPhrase");
      await cacheService.SetAsync("AuthenticationPassPhrase", secret, 8 * 60);
    }
    return secret ?? throw new KeyNotFoundException("auth pass phrase does not exist!");
  }

  public async Task<string> GetTwoFactorAuthenticationPassPhraseAsync()
  {
    var secret = await cacheService.GetAsync<string>("TwoFactorAuthenticationPassPhrase");
    if (secret == null)
    {
      secret = await secretManagementService.GetSecretAsync("TwoFactorAuthenticationPassPhrase");
      await cacheService.SetAsync("TwoFactorAuthenticationPassPhrase", secret, 8 * 60);
    }
    return secret ?? throw new KeyNotFoundException("2 factor auth pass phrase does not exist!");
  }

  public async Task<List<string>> GetPublicUrlsAsync()
  {
    return JsonConvert.DeserializeObject<List<string>>(configuration["PublicUrls"]);
  }

  public async Task<List<string>> GetCorsWhiteListAsync()
  {
    return JsonConvert.DeserializeObject<List<string>>(configuration["CorsWhiteList"]);
  }
}