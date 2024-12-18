using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Common.Auth.Dto;

public record AuthValidateRequest(bool Encrypted, string Token, string PassPhrase, string ValidAudience, string ValidIssuer, string Secret);
