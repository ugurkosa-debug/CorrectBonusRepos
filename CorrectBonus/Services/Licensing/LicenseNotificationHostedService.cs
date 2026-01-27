using CorrectBonus.Data;
using CorrectBonus.Services.Mail;
using CorrectBonus.Services.System;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.Licensing
{
    public class LicenseNotificationHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LicenseNotificationHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckLicensesAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckLicensesAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var mail = scope.ServiceProvider.GetRequiredService<IMailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<MailTemplateService>();
            var settings = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();

            var culture = settings.Get("System.DefaultCulture", "tr");
            var today = DateTime.UtcNow.Date;

            string L(string key)
                => settings.Get($"Licensing.LicenseNotification.{key}", key);

            var licenses = await db.TenantLicenses
                .Where(x =>
                    x.Status == "Active" &&
                    x.ExpireAt > today)
                .ToListAsync();

            foreach (var license in licenses)
            {
                var daysLeft = (license.ExpireAt.Date - today).Days;

                if (daysLeft != 30 && daysLeft != 7 && daysLeft != 1)
                    continue;

                if ((daysLeft == 30 && license.Notified30Days) ||
                    (daysLeft == 7 && license.Notified7Days) ||
                    (daysLeft == 1 && license.Notified1Day))
                    continue;

                var tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == license.TenantId);

                if (tenant == null || string.IsNullOrWhiteSpace(tenant.InvoiceEmail))
                    continue;

                var subject = daysLeft switch
                {
                    30 => L("Subject30"),
                    7 => L("Subject7"),
                    1 => L("Subject1"),
                    _ => "License Notification"
                };

                var values = new Dictionary<string, string>
                {
                    ["Culture"] = culture,
                    ["Subject"] = subject,
                    ["BodyTitle"] = L("BodyTitle"),
                    ["BodyGreeting"] = L("BodyGreeting")
                        .Replace("{CompanyName}", tenant.Name),
                    ["BodyText"] = L("BodyText")
                        .Replace("{DaysLeft}", daysLeft.ToString()),
                    ["BodyExpireDate"] = L("BodyExpireDate"),
                    ["ExpireDate"] = license.ExpireAt.ToString("dd.MM.yyyy"),
                    ["BodyAction"] = L("BodyAction"),
                    ["BodyFooter"] = L("BodyFooter"),
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                };

                var body = templateService.LoadTemplate(
                    "LicenseExpire.html",
                    values);

                await mail.SendAsync(
                    tenant.InvoiceEmail,
                    subject,
                    body);

                if (daysLeft == 30) license.Notified30Days = true;
                if (daysLeft == 7) license.Notified7Days = true;
                if (daysLeft == 1) license.Notified1Day = true;
            }

            await db.SaveChangesAsync();
        }
    }
}
