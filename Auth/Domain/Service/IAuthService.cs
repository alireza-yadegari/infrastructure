using Auth.Domain.Model;
using Auth.Domain.Dto;

namespace Auth.Domain.Service;
  internal interface IAuthService
  {
    Task<string> LoginAsync(LoginRequest model);
    Task<User> RegisterAsync(RegisterRequest model, string role);
    Task<Guid> GetUserIdFromTokenAsync(string encryptedToken);
  }