using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Models.UserManagement;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMailService _mailService;
        private readonly ILogService _logService;

        public UsersController(
            ApplicationDbContext context,
            IMailService mailService,
            ILogService logService)
        {
            _context = context;
            _mailService = mailService;
            _logService = logService;
        }

        [HttpGet]
        [RequirePermission("USERS_VIEW")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(x => x.Role)
                .OrderBy(x => x.FullName)
                .ToListAsync();

            return View(users.Select(x => new UserListVm
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                RoleName = x.Role.Name,
                IsActive = x.IsActive
            }).ToList());
        }

        [HttpGet]
        [RequirePermission("USERS_CREATE")]
        public async Task<IActionResult> Create()
        {
            var model = new UserCreateVm();
            await LoadRolesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("USERS_CREATE")]
        public async Task<IActionResult> Create(UserCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesAsync(model);
                return View(model);
            }

            if (await _context.Users.AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta zaten kayıtlı.");
                await LoadRolesAsync(model);
                return View(model);
            }

            var culture = HttpContext.Features
                .Get<IRequestCultureFeature>()?
                .RequestCulture
                .UICulture
                .TwoLetterISOLanguageName ?? "tr";

            var user = new Entities.Authorization.User
            {
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                PasswordHash = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>(),
                PasswordResetToken = Guid.NewGuid().ToString("N"),
                PasswordResetTokenExpireAt = DateTime.UtcNow.AddDays(1)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var setPasswordLink = Url.Action(
                "ResetPassword",
                "Auth",
                new { token = user.PasswordResetToken, culture },
                Request.Scheme);

            if (!string.IsNullOrWhiteSpace(setPasswordLink))
            {
                await _mailService.SendSetPasswordMailAsync(
                    user.Email,
                    user.FullName,
                    setPasswordLink,
                    culture);
            }

            await _logService.InfoAsync(
                "USERS_CREATE",
                "User created",
                user.Email);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequirePermission("USERS_EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound();

            var model = new UserEditVm
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };

            await LoadRolesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("USERS_EDIT")]
        public async Task<IActionResult> Edit(UserEditVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesAsync(model);
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            await _logService.InfoAsync("USERS_EDIT", "User updated", user.Email);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("USERS_STATUS")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRolesAsync(UserCreateVm model)
        {
            model.Roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToListAsync();
        }

        private async Task LoadRolesAsync(UserEditVm model)
        {
            model.Roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToListAsync();
        }
    }
}
