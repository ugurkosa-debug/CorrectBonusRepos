using CorrectBonus.Data;
using CorrectBonus.Entities.Authorization;
using CorrectBonus.Entities.System;
using CorrectBonus.Extensions;
using CorrectBonus.Models.Installer;
using CorrectBonus.Services.System.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using Net = global::System.Net;
using SIO = global::System.IO;

namespace CorrectBonus.Controllers.System
{
    [AllowAnonymous]
    public class InstallerController : Controller
    {
        private const string SESSION_KEY = "INSTALL_STATE";

        // ==================================================
        // 🔒 INSTALL LOCK
        // ==================================================
        private bool IsInstalled()
        {
            var path = SIO.Path.Combine(
                SIO.Directory.GetCurrentDirectory(),
                "App_Data",
                "installed.flag");

            return SIO.File.Exists(path);
        }

        private InstallState GetState()
            => HttpContext.Session.GetObject<InstallState>(SESSION_KEY)
               ?? new InstallState();

        private void SaveState(InstallState state)
            => HttpContext.Session.SetObject(SESSION_KEY, state);

        // ==================================================
        // 🔁 ALL GET → SINGLE WIZARD
        // ==================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (IsInstalled())
                return RedirectToAction("Login", "Auth");

            return View("Wizard", GetState());
        }

        [HttpGet] public IActionResult Server() => Index();
        [HttpGet] public IActionResult Login() => Index();
        [HttpGet] public IActionResult Database() => Index();
        [HttpGet] public IActionResult Mail() => Index();
        [HttpGet] public IActionResult Owner() => Index();
        [HttpGet] public IActionResult Finish() => Index();

        [HttpPost]
        public IActionResult NextStep()
        {
            var state = GetState();

            if (state.CurrentStep < 7)
                state.CurrentStep++;

            state.ErrorMessage = null;
            SaveState(state);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult PreviousStep()
        {
            var state = GetState();

            if (state.CurrentStep > 1)
                state.CurrentStep--;

            state.ErrorMessage = null;
            SaveState(state);

            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // STEP 1 – LANGUAGE
        // ==================================================
        [HttpPost]
        public IActionResult SelectLanguage(string language)
        {
            var state = GetState();

            state.Language = language;
            state.ErrorMessage = null;

            // 🔒 İlk kez dil seçiliyorsa step 1’de kal
            if (state.CurrentStep < 1)
                state.CurrentStep = 1;

            SaveState(state);

            var culture = language == "en" ? "en-US" : "tr-TR";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)));

            return RedirectToAction(nameof(Index));
        }



        // ==================================================
        // STEP 2 – SERVER TEST
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> TestServer(string server)
        {
            var state = GetState();
            state.Server = server;
            state.ErrorMessage = null;

            try
            {
                var cs =
                    $"Server={server};Database=master;" +
                    $"Trusted_Connection=True;" +
                    $"TrustServerCertificate=True;";

                using var conn = new SqlConnection(cs);
                await conn.OpenAsync();

                state.ServerConnected = true;

                // ✅ BAŞARILI → STEP 3
                state.CurrentStep = 3;
            }
            catch (Exception ex)
            {
                state.ServerConnected = false;
                state.ErrorMessage = ex.Message;
            }

            SaveState(state);
            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // STEP 3 – DB LOGIN
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> TestLogin(string dbUser, string dbPassword)
        {
            var state = GetState();
            state.DbUser = dbUser;
            state.DbPassword = dbPassword;
            state.ErrorMessage = null;

            try
            {
                var cs =
                    $"Server={state.Server};Database=master;" +
                    $"User Id={dbUser};Password={dbPassword};" +
                    $"TrustServerCertificate=True;";

                using var conn = new SqlConnection(cs);
                await conn.OpenAsync();

                state.DbAuthenticated = true;

                // ✅ BAŞARILI → STEP 4
                state.CurrentStep = 4;
            }
            catch (Exception ex)
            {
                state.DbAuthenticated = false;
                state.ErrorMessage = ex.Message;
            }

            SaveState(state);
            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // STEP 4 – DATABASE + MIGRATION
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> CreateDatabase(string databaseName)
        {
            var state = GetState();
            state.DatabaseName = databaseName;
            state.ErrorMessage = null;

            try
            {
                // 1️⃣ Master DB üzerinden veritabanı oluştur
                var masterCs =
                    $"Server={state.Server};Database=master;" +
                    $"User Id={state.DbUser};Password={state.DbPassword};" +
                    $"TrustServerCertificate=True;";

                using (var conn = new SqlConnection(masterCs))
                {
                    await conn.OpenAsync();

                    var safeDb = databaseName.Replace("]", "]]");

                    var sql = $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{safeDb}')
BEGIN
    CREATE DATABASE [{safeDb}]
END";

                    using var cmd = new SqlCommand(sql, conn);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2️⃣ Uygulama DB'sine bağlan ve migration çalıştır
                var appCs =
                    $"Server={state.Server};Database={databaseName};" +
                    $"User Id={state.DbUser};Password={state.DbPassword};" +
                    $"TrustServerCertificate=True;";

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(appCs)
                    .Options;

                await using var db = new ApplicationDbContext(options);
                await db.Database.MigrateAsync();

                state.DatabaseCreated = true;

                // ✅ BAŞARILI → STEP 5
                state.CurrentStep = 5;
            }
            catch (Exception ex)
            {
                state.DatabaseCreated = false;
                state.ErrorMessage = ex.Message;
            }

            SaveState(state);
            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // STEP 5 – MAIL TEST
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> TestMail(
    string smtpHost,
    int smtpPort,
    string smtpUser,
    string smtpPassword,
    bool enableSsl,
    string testEmail)
        {
            var state = GetState();
            state.ErrorMessage = null;

            try
            {
                using var client = new Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new Net.NetworkCredential(smtpUser, smtpPassword)
                };

                var msg = new Net.Mail.MailMessage(
                    smtpUser,
                    testEmail,
                    "CorrectBonus Test Mail",
                    "Mail ayarları başarıyla test edildi.");

                await client.SendMailAsync(msg);

                state.SmtpHost = smtpHost;
                state.SmtpPort = smtpPort;
                state.SmtpUser = smtpUser;
                state.SmtpPassword = smtpPassword;
                state.EnableSsl = enableSsl;
                state.MailTested = true;

                // ✅ BAŞARILI → STEP 6
                state.CurrentStep = 6;
            }
            catch (Exception ex)
            {
                state.MailTested = false;
                state.ErrorMessage = ex.Message;
            }

            SaveState(state);
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // STEP 6 – OWNER + FULL PERMISSION (FINAL - SAFE)
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> CreateOwner(string ownerEmail)
        {
            var state = GetState();
            state.OwnerEmail = ownerEmail;
            state.ErrorMessage = null;

            try
            {
                var cs =
                    $"Server={state.Server};Database={state.DatabaseName};" +
                    $"User Id={state.DbUser};Password={state.DbPassword};" +
                    $"TrustServerCertificate=True;";

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(cs)
                    .Options;

                await using var db = new ApplicationDbContext(options);

                // 1️⃣ SEED (ÖNCE)
                PermissionSeed.Seed(db);
                await new MenuSeed().ApplyAsync(db);

                // 2️⃣ OWNER ROLE (GARANTİLİ)
                var ownerRole = await db.Roles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Name == "Owner");

                if (ownerRole == null)
                {
                    ownerRole = new Role
                    {
                        Name = "Owner",
                        IsActive = true
                    };

                    db.Roles.Add(ownerRole);
                    await db.SaveChangesAsync(); // 🔴 BURASI KRİTİK
                }

                // 3️⃣ OWNER USER DUPLICATE KONTROL
                var existingUser = await db.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email == ownerEmail);

                if (existingUser != null)
                    throw new Exception("Bu e-posta ile bir kullanıcı zaten mevcut.");

                // 4️⃣ OWNER USER
                var user = new User
                {
                    Email = ownerEmail,
                    FullName = "Application Owner",
                    IsActive = true,
                    RoleId = ownerRole.Id, // 🔥 ARTIK GARANTİLİ
                    PasswordResetToken = Guid.NewGuid().ToString("N"),
                    PasswordResetTokenExpireAt = DateTime.UtcNow.AddHours(1)
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                // 5️⃣ PERMISSIONS (GARANTİLİ)
                var allPermissionIds = await db.Permissions
                    .IgnoreQueryFilters()
                    .Where(p => p.IsActive)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (!allPermissionIds.Any())
                    throw new Exception("Permission listesi boş. PermissionSeed çalışmamış.");

                var existingPermissionIds = await db.RolePermissions
                    .Where(rp => rp.RoleId == ownerRole.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var existingSet = new HashSet<int>(existingPermissionIds);

                foreach (var pid in allPermissionIds)
                {
                    if (!existingSet.Contains(pid))
                    {
                        db.RolePermissions.Add(new RolePermission
                        {
                            RoleId = ownerRole.Id,
                            PermissionId = pid
                        });
                    }
                }

                await db.SaveChangesAsync();

                // 6️⃣ RESET MAIL
                var resetLink = Url.Action(
                    "ResetPassword",
                    "Auth",
                    new { token = user.PasswordResetToken, culture = state.Language ?? "tr" },
                    Request.Scheme)!;

                await SendInstallerMailAsync(
                    state,
                    user.Email,
                    user.FullName,
                    resetLink);

                state.OwnerCreated = true;
                state.CurrentStep = 7;
            }
            catch (Exception ex)
            {
                state.OwnerCreated = false;
                state.ErrorMessage =
                    ex.InnerException?.Message ?? ex.Message; // 🔥 GERÇEK HATA
            }

            SaveState(state);
            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // STEP 7 – FINISH (FINAL)
        // ==================================================
        [HttpPost]
        public async Task<IActionResult> Complete()
        {
            var state = GetState();

            try
            {
                await SeedSystemAsync(state);

                var finalCs =
                    $"Server={state.Server};Database={state.DatabaseName};" +
                    $"User Id={state.DbUser};Password={state.DbPassword};" +
                    $"TrustServerCertificate=True;";

                var appSettingsPath = SIO.Path.Combine(
                    SIO.Directory.GetCurrentDirectory(),
                    "appsettings.json");

                var json = await SIO.File.ReadAllTextAsync(appSettingsPath);
                using var doc = JsonDocument.Parse(json);

                var dict = new Dictionary<string, object?>();

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.NameEquals("ConnectionStrings")) continue;
                    dict[prop.Name] =
                        JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                }

                dict["ConnectionStrings"] = new Dictionary<string, string>
        {
            { "DefaultConnection", finalCs }
        };

                await SIO.File.WriteAllTextAsync(
                    appSettingsPath,
                    JsonSerializer.Serialize(dict, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));

                var appDataPath = SIO.Path.Combine(
                    SIO.Directory.GetCurrentDirectory(),
                    "App_Data");

                SIO.Directory.CreateDirectory(appDataPath);

                await SIO.File.WriteAllTextAsync(
                    SIO.Path.Combine(appDataPath, "installed.flag"),
                    DateTime.UtcNow.ToString("O"));
            }
            catch (Exception ex)
            {
                state.ErrorMessage = "Kurulum sırasında hata oluştu: " + ex.Message;
                SaveState(state);
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.Remove(SESSION_KEY);
            return RedirectToAction("Login", "Auth");
        }


        // ==================================================
        // 🔑 SEED
        // ==================================================
        private async Task SeedSystemAsync(InstallState state)
        {
            var cs =
                $"Server={state.Server};Database={state.DatabaseName};" +
                $"User Id={state.DbUser};Password={state.DbPassword};" +
                $"TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(cs)
                .Options;

            await using var db = new ApplicationDbContext(options);

            PermissionSeed.Seed(db);          // 1️⃣
            await new MenuSeed().ApplyAsync(db); // 2️⃣
            await SaveMailSettingsAsync(db, state);
        }


        // ==================================================
        // 🔧 FINAL SETTINGS
        // ==================================================
        private async Task SaveFinalSettingsAsync(InstallState state)
        {
            var finalCs =
                $"Server={state.Server};Database={state.DatabaseName};" +
                $"User Id={state.DbUser};Password={state.DbPassword};" +
                $"TrustServerCertificate=True;";

            var appSettingsPath = SIO.Path.Combine(
                SIO.Directory.GetCurrentDirectory(),
                "appsettings.json");

            var json = await SIO.File.ReadAllTextAsync(appSettingsPath);
            using var doc = JsonDocument.Parse(json);

            var dict = new Dictionary<string, object?>();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.NameEquals("ConnectionStrings")) continue;
                dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
            }

            dict["ConnectionStrings"] = new Dictionary<string, string>
            {
                { "DefaultConnection", finalCs }
            };

            await SIO.File.WriteAllTextAsync(
                appSettingsPath,
                JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));

            var appDataPath = SIO.Path.Combine(
                SIO.Directory.GetCurrentDirectory(),
                "App_Data");

            SIO.Directory.CreateDirectory(appDataPath);

            await SIO.File.WriteAllTextAsync(
                SIO.Path.Combine(appDataPath, "installed.flag"),
                DateTime.UtcNow.ToString("O"));
        }

        // ==================================================
        // 🔔 MAIL
        // ==================================================
        private async Task SendInstallerMailAsync(
            InstallState state,
            string toEmail,
            string fullName,
            string resetLink)
        {
            using var client = new Net.Mail.SmtpClient(
                state.SmtpHost!,
                state.SmtpPort)
            {
                EnableSsl = state.EnableSsl,
                Credentials = new Net.NetworkCredential(
                    state.SmtpUser!,
                    state.SmtpPassword!)
            };

            var body =
                $"Merhaba {fullName},\n\n" +
                $"Şifrenizi oluşturmak için bağlantıya tıklayın:\n{resetLink}";

            var mail = new Net.Mail.MailMessage(
                state.SmtpUser!,
                toEmail,
                "CorrectBonus – Şifre Oluşturma",
                body);

            await client.SendMailAsync(mail);
        }

        // ==================================================
        // 🔐 SYSTEM SETTINGS – MAIL
        // ==================================================
        private async Task SaveMailSettingsAsync(
            ApplicationDbContext db,
            InstallState state)
        {
            await UpsertAsync(db, "Mail.SmtpHost", state.SmtpHost!);
            await UpsertAsync(db, "Mail.SmtpPort", state.SmtpPort.ToString());
            await UpsertAsync(db, "Mail.SmtpUsername", state.SmtpUser!);
            await UpsertAsync(db, "Mail.SmtpPassword", state.SmtpPassword!);
            await UpsertAsync(db, "Mail.EnableSsl", state.EnableSsl.ToString());
            await UpsertAsync(db, "Mail.FromAddress", state.SmtpUser!);
            await UpsertAsync(db, "Mail.FromName", "CorrectBonus");
            await db.SaveChangesAsync();
        }

        private async Task UpsertAsync(
            ApplicationDbContext db,
            string key,
            string value)
        {
            var setting = await db.SystemSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SettingKey == key);

            if (setting == null)
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = key,
                    Value = value,
                    Group = "Mail",
                    DescriptionEn = $"Mail setting: {key}",
                    DescriptionTr = $"Mail ayarı: {key}"
                });
            }
            else
            {
                setting.Value = value;
            }
        }
    }
}
