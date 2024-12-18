namespace Common.Configuration.Dto;

public record JWTInformation(string ValidAudience, string ValidIssuer, int TokenExpiryTimeInHour,string? Secret = null);