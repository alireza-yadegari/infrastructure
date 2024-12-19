    namespace Auth.Domain.Model;
    public record LoginRequest(string Username, string Password,string? twoFactorAuth);
