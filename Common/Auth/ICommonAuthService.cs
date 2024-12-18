using Common.Auth.Dto;
using Microsoft.IdentityModel.Tokens;

namespace Common.Auth;

  public interface ICommonAuthService
  {
    Task<TokenValidationResult> ValidateAsync(AuthValidateRequest request);
    Task<Guid> GetUserIdAsync(AuthGetUserIdRequest request);
    Task<string> GenerateTokenAsync(AuthGenerateTokenRequest request);
  }