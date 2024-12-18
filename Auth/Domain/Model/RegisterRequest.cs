namespace Auth.Domain.Model;
public record RegisterRequest(string Username, string Password, string Email, string Name, string? LastName,string? role);
