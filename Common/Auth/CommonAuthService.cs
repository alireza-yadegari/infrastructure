using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Common.Auth.Dto;
using Common.Encryption;
using Microsoft.IdentityModel.Tokens;

namespace Common.Auth;

public class CommonAuthService : ICommonAuthService
{
  private readonly IEncryptionService encryptionService;
  public CommonAuthService(
    IEncryptionService encryptionService
  )
  {
    this.encryptionService = encryptionService;
  }
  public async Task<TokenValidationResult> ValidateAsync(AuthValidateRequest request)
  {
    var token = request.Encrypted ? await encryptionService.DecryptAsync(request.Token, request.PassPhrase) : request.Token;

    var validateParameters = new TokenValidationParameters()
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidAudience = request.ValidAudience,
      ValidIssuer = request.ValidIssuer,
      ClockSkew = TimeSpan.Zero,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.Secret))
    };

    var tokenHandler = new JwtSecurityTokenHandler();


    return await tokenHandler.ValidateTokenAsync(token, validateParameters);
  }

  public async Task<Guid> GetUserIdAsync(AuthGetUserIdRequest request)
  {
    var validatedToken = await ValidateAsync(new AuthValidateRequest(request.Encrypted, request.Token, request.PassPhrase, request.ValidAudience, request.ValidIssuer, request.Secret));

    if (!validatedToken.IsValid)
    {
      throw new InvalidCredentialException("token is not valid");
    }

    return Guid.Parse(validatedToken.Claims.First(x => x.Key == ClaimTypes.UserData).Value.ToString() ?? string.Empty);
  }

  public async Task<string> GenerateTokenAsync(AuthGenerateTokenRequest request)
  {
    var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, request.Username),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

    foreach (var userRole in request.UserRoles)
    {
      authClaims.Add(new Claim(ClaimTypes.Role, userRole));
    }

    authClaims.Add(new Claim(ClaimTypes.UserData, request.UserId.ToString()));


    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.Secret ?? ""));
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Issuer = request.ValidIssuer,
      Audience = request.ValidAudience,
      Expires = DateTime.UtcNow.AddHours(request.TokenExpiryTimeInHour),
      SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
      Subject = new ClaimsIdentity(authClaims)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return await encryptionService.EncryptAsync(tokenString, request.PassPhrase);
  }
}
