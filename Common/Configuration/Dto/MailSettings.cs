namespace Common.Configuration.Dto;

public record MailSettings(string DisplayName, string Host, string Mail, int Port, string? Password = null);