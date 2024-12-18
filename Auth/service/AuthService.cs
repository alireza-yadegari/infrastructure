using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using Auth.Domain.Dto;
using Auth.Domain.Enumeration;
using Auth.Domain.Model;
using Auth.Domain.Service;
using Auth.Helpers;
using Common.Auth;
using Common.Configuration;
using Common.Encryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Auth.Service;
internal class AuthService : IAuthService
{

  private readonly UserManager<User> userManager;
  private readonly ICommonAuthService commonAuthService;
  private readonly IEncryptionService encryptionService;
  private readonly IConfigurationService configurationService;
  private readonly RoleManager<Role> roleManager;

  public AuthService(
              UserManager<User> userManager,
              IConfigurationService configurationService,
              ICommonAuthService commonAuthService,
              IEncryptionService encryptionService,
              RoleManager<Role> roleManager
  )
  {
    this.userManager = userManager;
    this.configurationService = configurationService;
    this.commonAuthService = commonAuthService;
    this.roleManager = roleManager;
    this.encryptionService = encryptionService;
  }
  public async Task<string> LoginAsync(LoginRequest model)
  {
    var user = await userManager.FindByNameAsync(model.Username);
    if (user == null)
      throw new LoginException("Invalid username");

    if (!await userManager.CheckPasswordAsync(user, model.Password))
      throw new LoginException("Invalid password");

    //if (!await userManager.IsEmailConfirmedAsync(user))
    //    throw new MafiaException("Email not confirmed!");

    if (!await userManager.GetLockoutEnabledAsync(user))
      throw new LoginException("User locked out!");



    var userRoles = await userManager.GetRolesAsync(user);

    var passPhrase = await configurationService.GetAuthenticationPassPhraseAsync();
    var jwt = await configurationService.GetJWTKeyAsync();

    string token = await commonAuthService.GenerateTokenAsync(new Common.Auth.Dto.AuthGenerateTokenRequest(passPhrase,jwt.ValidAudience,jwt.ValidIssuer,jwt.Secret,jwt.TokenExpiryTimeInHour,user.UserName,user.Id,userRoles?.ToList()));
    return token;
  }

  public async Task<User> RegisterAsync(RegisterRequest model, string role)
  {
    var userExists = await userManager.FindByNameAsync(model.Username);
    if (userExists != null)
      throw new RegisterException(JsonConvert.SerializeObject(new List<string>() { "User already exists" }));

    var emailLink = (await configurationService.GetTwoFactorAuthenticationAsync())?.EMailVerificationEndpoint;
    var emailVerificationUniqueId = Guid.NewGuid().ToString();

    StringBuilder stringBuilder = new StringBuilder();

    var passPhrase = await configurationService.GetTwoFactorAuthenticationPassPhraseAsync();

    stringBuilder.Append(emailLink);
    stringBuilder.Append("?");
    stringBuilder.Append($"user={await encryptionService.EncryptAsync(model.Username, passPhrase)}");
    stringBuilder.Append("&");
    stringBuilder.Append($"code={await encryptionService.EncryptAsync(emailVerificationUniqueId, passPhrase)}");


    User user = new()
    {
      Email = model.Email,
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = model.Username,
      Name = model.Name,
      Password = model.Password.HashPassword(),
      EmailConfirmed = false,
      ChangePassword = true,
      EmailConfirmationCode = emailVerificationUniqueId,
      LastName = model.LastName,

    };

    using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    {

      var createUserResult = await userManager.CreateAsync(user, model.Password);
      if (!createUserResult.Succeeded)
        throw new RegisterException(JsonConvert.SerializeObject(createUserResult.Errors.Select(x => x.Description).ToList()));

      if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new Role(role));

      if (await roleManager.RoleExistsAsync(role))
        await userManager.AddToRoleAsync(user, role);

      scope.Complete();

    }
    return user;
  }

  public async Task<Guid> GetUserIdFromTokenAsync(string encryptedToken)
  {
    var passPhrase = await configurationService.GetAuthenticationPassPhraseAsync();
    var jwt = await configurationService.GetJWTKeyAsync();

    if (jwt == null || jwt.Secret == null)
    {
      throw new KeyNotFoundException("there is no jwt configuration!");
    }
    var userId = await commonAuthService.GetUserIdAsync(new Common.Auth.Dto.AuthGetUserIdRequest(true,encryptedToken, passPhrase, jwt.ValidAudience, jwt.ValidIssuer, jwt.Secret));
    var user = await userManager.FindByIdAsync(userId.ToString());

    if (user == null)
    {
      throw new ValidateException("User Does not Exists");
    }
    return user.Id;
  }

}
