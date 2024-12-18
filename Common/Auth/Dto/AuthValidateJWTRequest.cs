using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Common.Auth.Dto;

public record AuthValidateJWTRequest(string Token, string ValidAudience, string ValidIssuer, string Secret);
