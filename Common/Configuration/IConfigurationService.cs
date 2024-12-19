
using Common.Configuration.Dto;

namespace Common.Configuration;
public interface IConfigurationService
{
  Task<CookieOption?> GetCookieOptionAsync();
  Task<AuthTwoFactorAuthentication?> GetTwoFactorAuthenticationAsync();
  Task<JWTInformation?> GetJWTKeyAsync();
  Task<string> GetAuthenticationTypeAsync();

  Task<List<string>> GetClientApiKeysAsync();

  Task<string> GetAuthenticationPassPhraseAsync();

  Task<string> GetTwoFactorAuthenticationPassPhraseAsync();

  Task<List<string>> GetPublicUrlsAsync();

  Task<List<string>> GetCorsWhiteListAsync();

  Task<MailSettings?> GetMailSettingsAsync();

  Task<string> GetCompanyNameAsync();
}