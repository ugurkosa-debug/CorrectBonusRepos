using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Models.Account;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IPasswordPolicyService _passwordPolicy;
        private readonly CurrentUserContext _currentUser;

        public AccountController(
            ApplicationDbContext context,
            ILogService logger,
            IWebHostEnvironment env,
            IPasswordPolicyService passwordPolicy,
            CurrentUserContext currentUser)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _passwordPolicy = passwordPolicy;
            _currentUser = currentUser;
        }

        // ==================================================
        // PROFILE (GET)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (_currentUser.UserId == null)
                return Forbid();

            // 1️⃣ Kullanıcı + Rol
            var dbUser = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value);

            if (dbUser == null || dbUser.Role == null)
                return Forbid();

            // 2️⃣ Yetkiler (AYRI QUERY – KRİTİK)
            var permissions = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == dbUser.RoleId)
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission.Code)
                .OrderBy(x => x)
                .ToListAsync();

            // 3️⃣ ViewModel
            var model = new ProfileVm
            {
                FullName = dbUser.FullName,
                Email = dbUser.Email,
                RoleName = dbUser.Role.Name,
                PreferredLanguage = dbUser.PreferredLanguage,
                ProfileImagePath = dbUser.ProfileImagePath,
                Permissions = permissions
            };

            ViewBag.DefaultAvatar = dbUser.Role.Name switch
            {
                "Admin" => "/images/avatars/admin.png",
                "Manager" => "/images/avatars/manager.png",
                _ => "/images/avatars/user.png"
            };

            return View(model);
        }

        // ==================================================
        // UPDATE PROFILE (LANGUAGE)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileVm model)
        {
            if (_currentUser.UserId == null)
                return Forbid();

            var user = await _context.Users.FindAsync(_currentUser.UserId.Value);
            if (user == null)
                return Forbid();

            user.PreferredLanguage = model.PreferredLanguage;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(model.PreferredLanguage))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(model.PreferredLanguage)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1)
                    });
            }

            return RedirectToAction(nameof(Profile));
        }

        // ==================================================
        // CHANGE PASSWORD
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid || _currentUser.UserId == null)
                return RedirectToAction(nameof(Profile));

            var user = await _context.Users.FindAsync(_currentUser.UserId.Value);
            if (user == null || user.PasswordHash == null || user.PasswordSalt == null)
                return Forbid();

            if (!PasswordHasher.Verify(
                model.CurrentPassword,
                user.PasswordHash,
                user.PasswordSalt))
            {
                TempData["Error"] = "Mevcut şifre yanlış.";
                return RedirectToAction(nameof(Profile));
            }

            var policyResult = await _passwordPolicy.ValidateAsync(
                model.NewPassword, user);

            if (!policyResult.IsValid)
            {
                TempData["Error"] = string.Join("<br/>", policyResult.Errors);
                return RedirectToAction(nameof(Profile));
            }

            PasswordHasher.CreateHash(
                model.NewPassword,
                out var hash,
                out var salt);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordExpireAt = _passwordPolicy.CalculateExpireDate();

            _context.UserPasswordHistories.Add(new UserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            HttpContext.Session.Clear();

            await _logger.InfoAsync(
                "USER_PASSWORD_CHANGED",
                "Kullanıcı şifresini değiştirdi",
                user.Email);

            TempData["Success"] = "Şifre değiştirildi. Lütfen tekrar giriş yapın.";
            return RedirectToAction("Login", "Auth");
        }

        // ==================================================
        // UPDATE PROFILE PHOTO
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePhoto(IFormFile profileImage)
        {
            if (_currentUser.UserId == null || profileImage == null || profileImage.Length == 0)
                return RedirectToAction(nameof(Profile));

            var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext))
            {
                TempData["Error"] = "Sadece JPG veya PNG.";
                return RedirectToAction(nameof(Profile));
            }

            var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadRoot);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            var user = await _context.Users.FindAsync(_currentUser.UserId.Value);
            if (user == null)
                return Forbid();

            user.ProfileImagePath = $"/uploads/profiles/{fileName}";
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("ProfileImage", user.ProfileImagePath);

            return RedirectToAction(nameof(Profile));
        }
    }
}
