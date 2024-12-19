using Common.Email.Dto;

namespace Common.Email;

public interface IEmailService
{
  Task SendEmailAsync(SendEmailRequest mailRequest);
}