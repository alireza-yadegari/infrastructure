using Auth.Domain.Enumeration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Domain.Dto;

internal record ValidateToken(SecurityToken Token, Guid userId,LoginStatus LoginStatus);