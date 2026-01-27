using CorrectBonus.Entities.System;

namespace CorrectBonus.Data.Seed
{
    public static class SystemSettingSeed
    {
        public static void Seed(ApplicationDbContext context)
        {
            // =========================
            // MAIL SETTINGS (ZORUNLU)
            // =========================
            AddIfNotExists(context, "Mail", "Mail.Host", "",
                "SMTP Sunucu", "SMTP Host");

            AddIfNotExists(context, "Mail", "Mail.Port", "587",
                "SMTP Port", "SMTP Port");

            AddIfNotExists(context, "Mail", "Mail.UserName", "",
                "SMTP Kullanıcı Adı", "SMTP Username");

            AddIfNotExists(context, "Mail", "Mail.Password", "",
                "SMTP Şifre", "SMTP Password");

            AddIfNotExists(context, "Mail", "Mail.EnableSsl", "true",
                "SSL Kullan", "Enable SSL");

            AddIfNotExists(context, "Mail", "Mail.FromAddress", "",
                "Gönderen E-Posta", "From Address");

            AddIfNotExists(context, "Mail", "Mail.FromName", "CorrectBonus",
                "Gönderen Adı", "From Name");

            // =========================
            // APPLICATION (MINIMUM)
            // =========================
            AddIfNotExists(context, "Application", "App.DefaultLanguage", "tr-TR",
                "Varsayılan Dil", "Default Language");

            context.SaveChanges();
        }

        // ==================================================
        // HELPER
        // ==================================================
        private static void AddIfNotExists(
            ApplicationDbContext context,
            string group,
            string key,
            string defaultValue,
            string tr,
            string en)
        {
            if (context.SystemSettings.Any(x => x.SettingKey == key))
                return;

            context.SystemSettings.Add(new SystemSetting
            {
                Group = group,
                SettingKey = key,
                Value = defaultValue,
                DefaultValue = defaultValue,
                DescriptionTr = tr,
                DescriptionEn = en,
                IsActive = true,
                IsDefault = true
            });
        }
    }
}
