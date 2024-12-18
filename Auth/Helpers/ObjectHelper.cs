using System.Security.Cryptography;
using Auth.Domain.Service;
using Common.Configuration;
using Common.Constants;

namespace Auth.Helpers;

internal static class ObjectHelper
{
  internal static string HashPassword(this string password)
  {
    byte[] salt;
    byte[] buffer2;
    if (password == null)
    {
      throw new ArgumentNullException("password");
    }
    using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8, HashAlgorithmName.SHA512))
    {
      salt = bytes.Salt;
      buffer2 = bytes.GetBytes(0x20);
    }
    byte[] dst = new byte[0x31];
    Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
    Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
    return Convert.ToBase64String(dst);
  }

  internal static async Task<Guid> ToUserId(this HttpRequest request, IAuthService authService, IConfigurationService configurationService)
  {
    var authType = await configurationService.GetAuthenticationTypeAsync();
    string? encryptedToken = null;

    if (authType == "Cookie")
      encryptedToken = request.Cookies?.FirstOrDefault(x => x.Key.Equals(GeneralConstant.AuthCookieName, StringComparison.InvariantCultureIgnoreCase)).Value;
    else
    {
      encryptedToken = request.Headers.Authorization;
      encryptedToken = encryptedToken?.Replace("Bearer ", "");
    }

    if (encryptedToken == null)
      throw new ValidateException("user is not logged in");
      
    var userId = await authService.GetUserIdFromTokenAsync(encryptedToken);

    return userId;
  }
}