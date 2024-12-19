namespace Common.Email.Dto;

public record SendEmailRequest(string To, string Subject, string Body, List<string>? AttachmentFilePaths);