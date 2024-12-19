using System.Net;
using System.Net.Mail;
using Common.Configuration;
using Common.Configuration.Dto;
using Common.Email.Dto;

namespace Common.Email;

public class EmailService : IEmailService
{
  private readonly IConfigurationService configurationService;

  public EmailService(IConfigurationService configurationService)
  {
    this.configurationService = configurationService;
  }
  public async Task SendEmailAsync(SendEmailRequest mailRequest)
  {
    var _mailSettings = await configurationService.GetMailSettingsAsync();

    var email = new MailMessage();
    email.From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName);
    email.To.Add(mailRequest.To);
    email.Subject = mailRequest.Subject;
    if (mailRequest.AttachmentFilePaths != null)
    {
      foreach (var file in mailRequest.AttachmentFilePaths)
      {
        Attachment attachment = new Attachment(file);
        email.Attachments.Add(attachment);
      }
    }

    email.IsBodyHtml = true;
    email.Body = mailRequest.Body;

    using var smtp = new SmtpClient();
    smtp.Host = _mailSettings.Host;
    smtp.Port = _mailSettings.Port;

    smtp.UseDefaultCredentials = false;
    smtp.EnableSsl = true;
    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
    smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
    smtp.Send(email);
    smtp.Dispose();
  }
}