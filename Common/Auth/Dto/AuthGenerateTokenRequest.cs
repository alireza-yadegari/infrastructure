using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Common.Auth.Dto;

public record AuthGenerateTokenRequest(string PassPhrase, string ValidAudience, string ValidIssuer, string Secret, int TokenExpiryTimeInHour, string Username, Guid UserId, List<string>? UserRoles);
