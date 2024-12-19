using System.Drawing;
using System.Text;
using System.Transactions;
using Auth.Domain.Dto;
using Auth.Domain.Model;
using Auth.Domain.Service;
using Auth.Helpers;
using Common.Auth;
using Common.Configuration;
using Common.Email;
using Common.Encryption;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Auth.Service;
internal class AuthService : IAuthService
{

  private readonly UserManager<User> userManager;
  private readonly ICommonAuthService commonAuthService;
  private readonly IEncryptionService encryptionService;
  private readonly IConfigurationService configurationService;
  private readonly RoleManager<Role> roleManager;
  private readonly SignInManager<User> signInManager;
  private readonly IEmailService emailService;

  public AuthService(
              UserManager<User> userManager,
              IConfigurationService configurationService,
              ICommonAuthService commonAuthService,
              IEncryptionService encryptionService,
              RoleManager<Role> roleManager,
              SignInManager<User> signInManager,
              IEmailService emailService
  )
  {
    this.userManager = userManager;
    this.configurationService = configurationService;
    this.commonAuthService = commonAuthService;
    this.roleManager = roleManager;
    this.encryptionService = encryptionService;
    this.signInManager = signInManager;
    this.emailService = emailService;
  }
  public async Task<string> LoginAsync(LoginRequest model)
  {
    var user = await userManager.FindByNameAsync(model.Username);
    if (user == null)
      throw new LoginException("Invalid username");

    if (!await userManager.CheckPasswordAsync(user, model.Password))
      throw new LoginException("Invalid password");

    if (!await userManager.IsEmailConfirmedAsync(user))
      throw new LoginException("Email not confirmed!");

    if (!await userManager.GetLockoutEnabledAsync(user))
      throw new LoginException("User locked out!");

    var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);

    if (twoFactorEnabled)
    {
      if (string.IsNullOrEmpty(model.twoFactorAuth))
      {
        await SendTwoFactorCodeAsync(user);
        throw new LoginException("User needs 2 factor Authentication!");
      }
      else
      {
        var validated = await userManager.VerifyTwoFactorTokenAsync(user, "Email", model.twoFactorAuth);
        if (!validated)
        {
          throw new LoginException("Two Factor Authentication is not valid");
        }
      }
    }

    var userRoles = await userManager.GetRolesAsync(user);

    var passPhrase = await configurationService.GetAuthenticationPassPhraseAsync();
    var jwt = await configurationService.GetJWTKeyAsync();

    string token = await commonAuthService.GenerateTokenAsync(new Common.Auth.Dto.AuthGenerateTokenRequest(passPhrase, jwt.ValidAudience, jwt.ValidIssuer, jwt.Secret, jwt.TokenExpiryTimeInHour, user.UserName, user.Id, userRoles?.ToList()));
    return token;
  }

  public async Task<AuthenticationProperties> ExternalLoginAsync(string provider, string redirectUrl)
  {
    return signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
  }

  public async Task ExternalLoginCallbackAsync()
  {
    var info = await signInManager.GetExternalLoginInfoAsync();
    if (info == null)
    {
      throw new ExternalLoginException("External login info not available.");
    }

    var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
    if (!result.Succeeded)
    {
      throw new ExternalLoginException($"There is no account for this user in {info.LoginProvider}");
    }
  }

  public async Task EnableTwoFactorAsync(Guid userId)
  {
    var user = await userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      throw new NotFoundUserException("User Not Found");
    }

    await userManager.SetTwoFactorEnabledAsync(user, true);
  }

  public async Task<User> RegisterAsync(RegisterRequest model, string role)
  {
    var userExists = await userManager.FindByNameAsync(model.Username);
    if (userExists != null)
      throw new RegisterException(JsonConvert.SerializeObject(new List<string>() { "User already exists" }));

    User user = new()
    {
      Email = model.Email,
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = model.Username,
      Name = model.Name,
      Password = model.Password.HashPassword(),
      ChangePassword = true,
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

    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
    await SendEMailConfirmation(user.UserName, $"{user.Name} {user.LastName}", user.Email, token);
    return user;
  }

  public async Task ConfirmAccount(string encryptedUsername, string token)
  {
    var passPhrase = await configurationService.GetTwoFactorAuthenticationPassPhraseAsync();
    var username = await encryptionService.DecryptAsync(encryptedUsername, passPhrase);
    var user = await userManager.FindByNameAsync(username);
    if (user == null)
    {
      throw new RegisterException("user not valid");
    }

    var result = await userManager.ConfirmEmailAsync(user, token);
    if (result.Errors?.Any() ?? false)
    {
      throw new RegisterException(JsonConvert.SerializeObject(result.Errors));
    }
  }

  public async Task<Guid> GetUserIdFromTokenAsync(string encryptedToken)
  {
    var passPhrase = await configurationService.GetAuthenticationPassPhraseAsync();
    var jwt = await configurationService.GetJWTKeyAsync();

    if (jwt == null || jwt.Secret == null)
    {
      throw new KeyNotFoundException("there is no jwt configuration!");
    }
    var userId = await commonAuthService.GetUserIdAsync(new Common.Auth.Dto.AuthGetUserIdRequest(true, encryptedToken, passPhrase, jwt.ValidAudience, jwt.ValidIssuer, jwt.Secret));
    var user = await userManager.FindByIdAsync(userId.ToString());

    if (user == null)
    {
      throw new ValidateException("User Does not Exists");
    }
    return user.Id;
  }


  private async Task SendTwoFactorCodeAsync(User user)
  {
    var providers = await userManager.GetValidTwoFactorProvidersAsync(user);

    if (!providers.Contains("Email"))
    {
      throw new LoginException("Invalid 2-Factor authentication provider");
    }

    var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
    await emailService.SendEmailAsync(new Common.Email.Dto.SendEmailRequest(user.Email, "Login One Time Password", token, null));

  }
  private async Task SendEMailConfirmation(string username, string fullname, string email, string token)
  {
    StringBuilder stringBuilder = new StringBuilder();
    var emailLink = (await configurationService.GetTwoFactorAuthenticationAsync())?.EMailVerificationEndpoint;

    var passPhrase = await configurationService.GetTwoFactorAuthenticationPassPhraseAsync();
    var companyName = await configurationService.GetCompanyNameAsync();

    stringBuilder.Append(emailLink);
    stringBuilder.Append("?");
    stringBuilder.Append($"user={await encryptionService.EncryptAsync(username, passPhrase)}");
    stringBuilder.Append("&");
    stringBuilder.Append($"token={token}");

    var emailTemplate = File.ReadAllText("../Auth/Constants/EMailTemplate/email-confirmation-template.html");
    var emailBody = emailTemplate.Replace("{{CONFIRMATION_EMAIL_ADDRESS}}", stringBuilder.ToString());
    emailBody = emailBody.Replace("{{COMPANY_NAME}}", companyName);
    emailBody = emailBody.Replace("{{FULL_NAME}}", fullname);

    await emailService.SendEmailAsync(new Common.Email.Dto.SendEmailRequest(email, "Confirm Email Address", emailBody, null));

  }

}
