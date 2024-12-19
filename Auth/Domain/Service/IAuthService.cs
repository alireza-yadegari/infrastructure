using Auth.Domain.Model;
using Auth.Domain.Dto;
using Microsoft.AspNetCore.Authentication;

namespace Auth.Domain.Service;
  internal interface IAuthService
  {
    Task<UserLoginResponse> LoginAsync(LoginRequest model);
    Task<User> RegisterAsync(RegisterRequest model, string role);
    Task<Guid> GetUserIdFromTokenAsync(string encryptedToken);
    Task EnableTwoFactorAsync(Guid userId);
    Task ConfirmAccount(string encryptedUsername, string encryptedCode);
  }