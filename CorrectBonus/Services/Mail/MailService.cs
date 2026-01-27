using CorrectBonus.Services.Logs;
using CorrectBonus.Services.System;
using System.Net;
using System.Net.Mail;

namespace CorrectBonus.Services.Mail
{
    public class MailService : IMailService
    {
        private readonly ISystemSettingService _settings;
        private readonly MailTemplateService _templates;
        private readonly ILogService _logger;

        public MailService(
            ISystemSettingService settings,
            MailTemplateService templates,
            ILogService logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _templates = templates ?? throw new ArgumentNullException(nameof(templates));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==================================================
        // CORE SEND
        // ==================================================
        public async Task SendAsync(
            string to,
            string subject,
            string body)
        {
            var fromAddress = _settings.Get("Mail.FromAddress");
            var fromName = _settings.Get("Mail.FromName");

            if (string.IsNullOrWhiteSpace(fromAddress))
                throw new InvalidOperationException("Mail.FromAddress ayarlı değil.");

            using var smtp = new SmtpClient(
                _settings.Get("Mail.SmtpHost"),
                _settings.Get<int>("Mail.SmtpPort", 25))
            {
                EnableSsl = _settings.Get<bool>("Mail.EnableSsl", false),
                Credentials = new NetworkCredential(
                    _settings.Get("Mail.SmtpUsername"),
                    _settings.Get("Mail.SmtpPassword"))
            };

            using var mail = new MailMessage(
                new MailAddress(fromAddress, fromName),
                new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(mail);

            await _logger.InfoAsync(
                "MAIL_SEND_SUCCESS",
                $"Mail gönderildi: {to}",
                to
            );
        }

        // ==================================================
        // PASSWORD RESET (RESET + LOCK ACCOUNT)
        // ==================================================
        public async Task SendPasswordResetMailAsync(
            string toEmail,
            string fullName,
            string resetLink,
            string culture)
        {
            // 🔒 Güvenlik linki (aynı token ile)
            var lockAccountLink = resetLink
                .Replace("/ResetPassword", "/LockAccount");

            // 🔤 Template values
            var values = _templates.GetResetPasswordValues(
                culture,
                fullName,
                resetLink,
                lockAccountLink);

            var body = _templates.LoadTemplate(
                "PasswordResetMail.html",
                values);

            // 🔐 Subject fallback
            values.TryGetValue("Title", out var subject);
            subject ??= culture == "tr"
                ? "Şifre Sıfırlama"
                : "Password Reset";

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        // ==================================================
        // SET PASSWORD
        // ==================================================
        public async Task SendSetPasswordMailAsync(
            string toEmail,
            string fullName,
            string setPasswordLink,
            string culture)
        {
            var values = _templates.GetSetPasswordValues(
                culture,
                fullName,
                setPasswordLink);

            var body = _templates.LoadTemplate(
                "SetPassword.html",
                values);

            // 🔐 Subject fallback
            values.TryGetValue("Title", out var subject);
            subject ??= culture == "tr"
                ? "Şifre Oluşturma"
                : "Set Password";

            await SendAsync(
                toEmail,
                subject,
                body);
        }
    }
}
