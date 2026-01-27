using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Models.SystemManagement;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Mail;
using CorrectBonus.Services.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    [Authorize]
    public class SystemSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logger;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _env;
        private readonly ISystemSettingService _settings;

        public SystemSettingsController(
            ApplicationDbContext context,
            ILogService logger,
            IMailService mailService,
            IWebHostEnvironment env,
            ISystemSettingService settings)
        {
            _context = context;
            _logger = logger;
            _mailService = mailService;
            _env = env;
            _settings = settings;
        }

        // ==================================================
        // INDEX (PAGE)
        // ==================================================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Logs.Export)]
        public IActionResult Index()
        {
            var settings = _context.SystemSettings
                .AsNoTracking()
                .OrderBy(x => x.Group)
                .ThenBy(x => x.SettingKey)
                .ToList();

            return View(settings
                .GroupBy(x => x.Group)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new SystemSettingEditVm
                    {
                        Id = x.Id,
                        Group = x.Group,
                        Key = x.SettingKey,
                        Value = x.Value,
                        DescriptionTr = x.DescriptionTr,
                        DescriptionEn = x.DescriptionEn,
                        IsActive = x.IsActive
                    }).ToList()
                ));
        }

        // ==================================================
        // SAVE GROUP
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.System.SettingsEdit)]
        public async Task<IActionResult> SaveGroup(
            string group,
            List<SystemSettingEditVm> model)
        {
            foreach (var item in model)
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (setting == null)
                    continue;

                setting.Value = item.Value ?? "";
                setting.IsActive = true;
                setting.IsDefault = false;
            }

            await _context.SaveChangesAsync();

            await _logger.InfoAsync(
                "SYSTEM_SETTINGS_UPDATED",
                $"{group} ayarları güncellendi",
                User.Identity?.Name
            );

            TempData["Success"] = $"{group} ayarları kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // FILE UPLOAD (LOGO / BG / FAVICON)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.System.SettingsEdit)]
        public async Task<IActionResult> UploadApplicationAsset(
            string key,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            string folder;
            string fileName;

            switch (key)
            {
                case "App.LogoPath":
                    folder = "images/logo";
                    fileName = "app-logo" + ext;
                    break;

                case "App.FaviconPath":
                    folder = "";
                    fileName = "favicon" + ext;
                    break;

                case "App.LoginBackgroundPath":
                    folder = "images/login";
                    fileName = "login-bg" + ext;
                    break;

                default:
                    return BadRequest("Geçersiz anahtar");
            }

            var physicalFolder = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(physicalFolder);

            var fullPath = Path.Combine(physicalFolder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var setting = await _context.SystemSettings
                .FirstAsync(x => x.SettingKey == key);

            setting.Value = "/" + Path
                .Combine(folder, fileName)
                .Replace("\\", "/");

            setting.IsDefault = false;

            await _context.SaveChangesAsync();

            await _logger.InfoAsync(
                "SYSTEM_ASSET_UPLOADED",
                $"{key} güncellendi -> {setting.Value}",
                User.Identity?.Name
            );

            return Ok();
        }

        // ==================================================
        // TEST MAIL
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.System.SettingsEdit)]
        public async Task<IActionResult> TestMail()
        {
            try
            {
                var to = await _context.SystemSettings
                    .Where(x => x.SettingKey == "Mail.FromAddress")
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(to))
                    return Json(new
                    {
                        success = false,
                        message = "Mail adresi tanımlı değil."
                    });

                await _mailService.SendAsync(
                    to,
                    "Test Mail",
                    "<b>Mail ayarları çalışıyor.</b>");

                return Json(new
                {
                    success = true,
                    message = "Mail gönderildi."
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(
                    "MAIL_TEST_FAILED",
                    ex,
                    User.Identity?.Name);

                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==================================================
        // RESET THEME
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.System.SettingsEdit)]
        public IActionResult ResetTheme()
        {
            _settings.ResetThemeToDefault();
            TempData["Success"] = "Tema varsayılana döndürüldü.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [RequirePermission(PermissionRegistry.System.SettingsEdit)]
        public IActionResult ResetThemeAjax()
        {
            _settings.ResetThemeToDefault();
            return Ok();
        }
    }
}
