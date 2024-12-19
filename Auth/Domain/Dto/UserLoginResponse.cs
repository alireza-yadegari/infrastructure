using Auth.Domain.Enumeration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Domain.Dto;

internal record UserLoginResponse(string Token,LoginStatus Status, string FullName);