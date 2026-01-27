namespace CorrectBonus.Services.Mail
{
    public interface IMailService
    {
        Task SendAsync(string to, string subject, string body);

        Task SendPasswordResetMailAsync(
            string email,
            string fullName,
            string resetLink,
            string culture);

        Task SendSetPasswordMailAsync(
            string email,
            string fullName,
            string setPasswordLink,
            string culture);
    }
}
