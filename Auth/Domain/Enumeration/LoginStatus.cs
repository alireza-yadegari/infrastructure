namespace Auth.Domain.Enumeration;

  public enum LoginStatus
  {
    NotLoggedIn = 1,
    LoggedIn = 2,
    NeedChangePassword = 3,
    NeedTwoFactorAuthenticationCode = 4,
    TwoFactorAuthenticationCodeNotValid = 5
  }