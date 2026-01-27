using System.Globalization;
using System.Resources;
using System.Text;

namespace CorrectBonus.Services.Mail
{
    public class MailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public MailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }


        // ==================================================
        // HTML TEMPLATE LOADER
        // ==================================================
        public string LoadTemplate(string templateName, Dictionary<string, string> values)
        {
            var path = Path.Combine(
                _env.ContentRootPath,
                "Services",
                "Mail",
                "Templates",
                templateName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Mail template bulunamadı: {templateName}");

            var html = File.ReadAllText(path, Encoding.UTF8);

            foreach (var item in values)
            {
                html = html.Replace($"{{{{{item.Key}}}}}", item.Value ?? string.Empty);
            }

            return html;
        }

        // ==================================================
        // PASSWORD RESET VALUES
        // ==================================================
        public Dictionary<string, string> GetResetPasswordValues(
            string culture,
            string fullName,
            string resetLink)
        {
            var rm = new ResourceManager(
                "CorrectBonus.Resources.Mail.PasswordReset",
                typeof(MailTemplateService).Assembly);

            var policyItems = new[]
            {
                rm.GetString("PolicyMinLength"),
                rm.GetString("PolicyUpper"),
                rm.GetString("PolicyLower"),
                rm.GetString("PolicyNumber"),
                rm.GetString("PolicySpecial")
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => $"<li>{x}</li>");

            return new Dictionary<string, string>
            {
                ["Title"] = rm.GetString("Title") ?? "Şifre Sıfırlama",
                ["Greeting"] = string.Format(
                    rm.GetString("Greeting") ?? "Merhaba {0},",
                    fullName),

                ["Description"] = rm.GetString("Description") ?? "",
                ["PolicyTitle"] = rm.GetString("PolicyTitle") ?? "",
                ["PolicyItems"] = string.Join("", policyItems),
                ["ButtonText"] = rm.GetString("ButtonText") ?? "Şifreyi Sıfırla",
                ["SecurityNote"] = rm.GetString("SecurityNote") ?? "",
                ["FooterNote"] = rm.GetString("FooterNote") ?? "",
                ["ResetLink"] = resetLink,
                ["Culture"] = culture
            };
        }

        // ==================================================
        // SET PASSWORD VALUES (ileride)
        // ==================================================
        public Dictionary<string, string> GetSetPasswordValues(
            string culture,
            string fullName,
            string setPasswordLink)
        {
            return new Dictionary<string, string>
            {
                ["Title"] = culture == "tr" ? "Şifre Oluşturma" : "Set Password",
                ["FullName"] = fullName,
                ["SetPasswordLink"] = setPasswordLink,
                ["ButtonText"] = culture == "tr" ? "Şifre Oluştur" : "Set Password"
            };
        }

        // ==================================================
        // RESOURCE MANAGER
        // ==================================================
        private ResourceManager GetPasswordResetResourceManager(string culture)
        {
            var ci = new CultureInfo(
                string.IsNullOrWhiteSpace(culture) ? "tr" : culture);

            return new ResourceManager(
                "CorrectBonus.Resources.Mail.PasswordReset",
                typeof(MailTemplateService).Assembly);
        }
        public Dictionary<string, string> GetResetPasswordValues(
    string culture,
    string fullName,
    string resetLink,
    string lockAccountLink)
        {
            var ci = new CultureInfo(string.IsNullOrWhiteSpace(culture) ? "tr" : culture);

            var rm = new ResourceManager(
                "CorrectBonus.Resources.Mail.PasswordReset",
                typeof(MailTemplateService).Assembly);

            return new Dictionary<string, string>
            {
                ["Title"] = rm.GetString("Title", ci) ?? "Şifre Sıfırlama",
                ["Greeting"] = rm.GetString("Greeting", ci) ?? $"Merhaba {fullName},",
                ["Description"] = rm.GetString("Description", ci) ?? "",

                // 🔐 POLİCY
                ["PolicyTitle"] = rm.GetString("PolicyTitle", ci) ?? "Şifre Güvenlik Kuralları",
                ["PolicyItems"] = rm.GetString("PolicyItems", ci) ??
                    "<ul style='padding-left:16px;margin:0'>" +
                    "<li>En az 8 karakter</li>" +
                    "<li>En az 1 büyük harf</li>" +
                    "<li>En az 1 küçük harf</li>" +
                    "<li>En az 1 rakam</li>" +
                    "<li>En az 1 özel karakter</li>" +
                    "</ul>",

                ["ButtonText"] = rm.GetString("ButtonText", ci) ?? "Şifreyi Sıfırla",

                // 🔒 SECURITY
                ["SecurityNote"] = rm.GetString("SecurityNote", ci)
                    ?? "Bu işlemi siz başlatmadıysanız hesabınızı hemen kilitlemenizi öneririz.",

                ["LockAccountText"] = rm.GetString("LockAccountText", ci)
                    ?? "Bu işlemi ben yapmadıysam hesabımı kilitle",

                ["FooterNote"] = rm.GetString("FooterNote", ci)
                    ?? "Bu e-posta otomatik olarak gönderilmiştir.",

                ["ResetLink"] = resetLink,
                ["LockAccountLink"] = lockAccountLink
            };
        }


    }
}
