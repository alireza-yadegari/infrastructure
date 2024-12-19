using Auth.Domain.Enumeration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Domain.Dto;

internal record UserRegisterResponse(string Name,string LastName,bool NeedConfirmation);