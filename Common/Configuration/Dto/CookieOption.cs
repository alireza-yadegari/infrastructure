namespace Common.Configuration.Dto;

public record CookieOption(DateTime Expires,bool HttpOnly,string Path,bool Secure,string SameSite,string? Domain);