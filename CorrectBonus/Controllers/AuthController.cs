using Azure;
using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Models.Auth;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Mail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;
        private readonly IMailService _mailService;
        private readonly ILogService _logService;
        private readonly IPasswordPolicyService _passwordPolicy;
        private readonly IStringLocalizer<AuthController> _L;

        public AuthController(
            ApplicationDbContext db,
            CurrentUserContext currentUser,
            IMailService mailService,
            ILogService logService,
            IPasswordPolicyService passwordPolicy,
            IStringLocalizer<AuthController> L)
        {
            _db = db;
            _currentUser = currentUser;
            _mailService = mailService;
            _logService = logService;
            _passwordPolicy = passwordPolicy;
            _L = L;
        }

        // ==================================================
        // LOGIN
        // ==================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginVm());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLower();

            var user = await _db.Users
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                ModelState.AddModelError("", _L["UserNotFound"]);
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", _L["UserPassive"]);
                return View(model);
            }

            if (user.PasswordHash == null || user.PasswordSalt == null)
            {
                ModelState.AddModelError("", _L["PasswordNotSet"]);
                return View(model);
            }

            if (!VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", _L["WrongPassword"]);
                return View(model);
            }

            if (user.PasswordExpireAt.HasValue && user.PasswordExpireAt < DateTime.UtcNow)
            {
                user.IsActive = false;
                await _db.SaveChangesAsync();
                ModelState.AddModelError("", _L["PasswordExpired"]);
                return View(model);
            }

            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // ==================================================
        // FORGOT PASSWORD
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVm model)
        {
            if (!ModelState.IsValid)
                return BadRequest(_L["InvalidEmail"].Value);

            var email = model.Email.Trim().ToLower();

            var user = await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
                return NotFound(_L["UserNotFound"].Value);

            if (!user.IsActive)
                return BadRequest(_L["UserPassiveContactAdmin"].Value);

            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetTokenExpireAt = DateTime.UtcNow.AddHours(1);

            await _db.SaveChangesAsync();

            var culture = GetCurrentCulture();

            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new { token = user.PasswordResetToken, culture },
                Request.Scheme)!;

            await _mailService.SendPasswordResetMailAsync(
                user.Email,
                user.FullName,
                resetLink,
                culture);

            return Ok(_L["ResetMailSent"].Value);
        }

        // ==================================================
        // RESET PASSWORD (GET)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string? culture)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest();

            if (!string.IsNullOrEmpty(culture))
                SetCultureCookie(culture);

            var user = await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x =>
                    x.PasswordResetToken == token &&
                    x.PasswordResetTokenExpireAt > DateTime.UtcNow);

            if (user == null)
                return View("ResetPasswordInvalid");

            return View(new ResetPasswordVm { Token = token });
        }

        // ==================================================
        // RESET PASSWORD (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x =>
                    x.PasswordResetToken == model.Token &&
                    x.PasswordResetTokenExpireAt > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", _L["InvalidResetLink"]);
                return View(model);
            }

            if (user.PasswordHash != null &&
                user.PasswordSalt != null &&
                VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", _L["NewPasswordCannotBeSameAsOld"]);
                return View(model);
            }

            var policyResult = await _passwordPolicy.ValidateAsync(model.Password, user);
            if (!policyResult.IsValid)
            {
                foreach (var err in policyResult.Errors)
                    ModelState.AddModelError("", err);

                return View(model);
            }

            CreatePasswordHash(model.Password, out var hash, out var salt);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordExpireAt = _passwordPolicy.CalculateExpireDate();
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpireAt = null;
            user.IsActive = true;

            _db.UserPasswordHistories.Add(new UserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }
[AllowAnonymous]
    [HttpGet]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Login));
    }


        // ==================================================
        // HELPERS
        // ==================================================
        private async Task SignInUserAsync(Entities.Authorization.User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("RoleId", user.RoleId.ToString()),
        new Claim("IsOwner", user.TenantId == null ? "true" : "false")
    };

            if (user.TenantId != null)
            {
                claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // ================================
            // ✅ SIDEBAR / UI İÇİN SESSION
            // ================================
            HttpContext.Session.SetString("FullName", user.FullName);

            HttpContext.Session.SetString(
                "TenantName",
                user.Role?.Name ?? ""
            );

            HttpContext.Session.SetString(
                "ProfileImage",
                user.ProfileImagePath ?? "/images/avatars/user.png"
            );
        }



        private bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private void SetCultureCookie(string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });
        }


        private string GetCurrentCulture()
        {
            return HttpContext.Features
                .Get<IRequestCultureFeature>()?
                .RequestCulture
                .UICulture
                .TwoLetterISOLanguageName ?? "tr";
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
