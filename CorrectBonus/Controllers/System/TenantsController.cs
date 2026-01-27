using CorrectBonus.Data;
using CorrectBonus.Entities.Authorization;
using CorrectBonus.Models.System;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Licensing;
using CorrectBonus.Services.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Globalization;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers.System
{
    using Microsoft.EntityFrameworkCore.Storage;

    [Authorize]
    public class TenantsController : Controller

    {
        private readonly ApplicationDbContext _context;
        private readonly CurrentUserContext _currentUser;
        private readonly IMailService _mailService;
        private readonly LicenseValidator _licenseValidator;

        public TenantsController(
            ApplicationDbContext context,
            CurrentUserContext currentUser,
            IMailService mailService,
            LicenseValidator licenseValidator)
        {
            _context = context;
            _currentUser = currentUser;
            _mailService = mailService;
            _licenseValidator = licenseValidator;
        }

        private IActionResult OwnerOnly()
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        // ==================================================
        // LIST
        // ==================================================
        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(t => new
                {
                    Tenant = t,
                    License = _context.TenantLicenses
                        .Where(l =>
                            l.TenantId == t.Id &&
                            l.Status == "Active" &&
                            l.LicenseKey.StartsWith("CBX1.") // 🔴 KRİTİK FİLTRE
                        )
                        .OrderByDescending(l => l.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var result = new List<TenantListVm>();

            foreach (var item in tenants)
            {
                var vm = new TenantListVm
                {
                    Id = item.Tenant.Id,
                    Code = item.Tenant.Code,
                    Name = item.Tenant.Name,
                    IsActive = item.Tenant.IsActive,
                    CreatedAt = item.Tenant.CreatedAt,
                    LicenseStatus = item.License != null ? "Active" : "None",
                    ExpireAt = item.License?.ExpireAt,
                    ActiveModules = new List<string>()
                };

                if (item.License != null)
                {
                    var payload = _licenseValidator.Validate(item.License.LicenseKey);

                    if (payload?.Modules != null && payload.Modules.Any())
                    {
                        vm.ActiveModules = payload.Modules.ToList();
                    }
                }

                result.Add(vm);
            }

            return View(result);
        }


        // ==================================================
        // CREATE (GET)
        // ==================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TenantCreateVm());
        }

        // ==================================================
        // CREATE (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Tenants.AnyAsync(x => x.Code == model.Code))
            {
                ModelState.AddModelError(nameof(model.Code), "Bu kod zaten kullanılıyor.");
                return View(model);
            }

            if (await _context.Users.AnyAsync(x => x.Email == model.AdminEmail))
            {
                ModelState.AddModelError(nameof(model.AdminEmail), "Bu email zaten kullanılıyor.");
                return View(model);
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var tenant = new Tenant
                {
                    Code = model.Code.Trim(),
                    Name = model.Name.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                var adminRole = new Role
                {
                    Name = "Admin",
                    TenantId = tenant.Id,
                    IsActive = true
                };

                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();

                var adminUser = new User
                {
                    FullName = model.AdminFullName.Trim(),
                    Email = model.AdminEmail.Trim(),
                    TenantId = tenant.Id,
                    RoleId = adminRole.Id,
                    IsActive = true,
                    PasswordHash = Array.Empty<byte>(),
                    PasswordSalt = Array.Empty<byte>(),
                    PasswordResetToken = Guid.NewGuid().ToString("N"),
                    PasswordResetTokenExpireAt = DateTime.UtcNow.AddHours(24)
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                // ✅ Transaction burada ve sadece burada commit edilir
                await tx.CommitAsync();
            }
            catch
            {
                // 🔑 Transaction hâlâ canlıysa rollback
                if (tx.GetDbTransaction().Connection != null)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }

            // 🔔 Mail gönderimi transaction DIŞINDA
            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new { token = model.AdminEmail },
                Request.Scheme)!;

            await _mailService.SendPasswordResetMailAsync(
                model.AdminEmail,
                model.AdminFullName,
                resetLink,
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            );

            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // TOGGLE ACTIVE
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            tenant.IsActive = !tenant.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // LICENSE (GET)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> License(int id)
        {

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            var license = await _context.TenantLicenses
                .Where(x => x.TenantId == id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            var model = new TenantLicenseVm
            {
                LicenseId = license?.Id ?? 0,
                TenantId = id,
                PublicKey = license?.PublicKey ?? "",
                Status = license?.Status ?? "Active"
            };


            ViewBag.TenantName = tenant.Name;

            return View(model);
        }

        // ==================================================
        // LICENSE (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> License(TenantLicenseVm model)
        {

            if (!ModelState.IsValid)
                return View(model);

            // 🔑 Kullanıcı kısa lisansı girer
            var license = await _context.TenantLicenses
                .FirstOrDefaultAsync(x =>
                    x.PublicKey == model.PublicKey.Trim() &&
                    x.TenantId == model.TenantId);

            if (license == null)
            {
                ModelState.AddModelError(
                    nameof(model.PublicKey),
                    "Geçersiz lisans anahtarı."
                );
                return View(model);
            }

            var payload = _context
                .GetService<LicenseValidator>()
                .Validate(license.LicenseKey);

            if (payload == null)
            {
                ModelState.AddModelError(
                    nameof(model.PublicKey),
                    "Lisans doğrulanamadı."
                );
                return View(model);
            }

            license.Status = "Active";
            license.ExpireAt = payload.Exp;

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tenant == null)
                return NotFound();

            // 🔑 Tenant admin / yetkili kullanıcı
            var authorizedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tenant.Id &&
                    x.IsActive);

            var vm = new TenantDetailVm
            {
                Id = tenant.Id,
                Name = tenant.Name,
                TaxNumber = null,      // şimdilik boş – alan hazır
                Address = null,

                AuthorizedUserId = authorizedUser?.Id ?? 0,
                AuthorizedFullName = authorizedUser?.FullName,
                AuthorizedEmail = authorizedUser?.Email,
                AuthorizedPhone = null,

                InvoiceTitle = tenant.Name,
                InvoiceEmail = authorizedUser?.Email
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detail(TenantDetailVm model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var tenant = await _context.Tenants.FindAsync(model.Id);
            if (tenant == null)
                return NotFound();

            // ✅ SADECE ENTITY'DE OLAN ALANLAR
            tenant.Name = model.Name.Trim();

            // ❌ Bunlar şimdilik entity'de YOK
            // tenant.TaxNumber = ...
            // tenant.Address = ...

            if (model.AuthorizedUserId > 0)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == model.AuthorizedUserId);

                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.AuthorizedFullName))
                        user.FullName = model.AuthorizedFullName.Trim();

                    if (!string.IsNullOrWhiteSpace(model.AuthorizedEmail))
                        user.Email = model.AuthorizedEmail.Trim();
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] = "TenantUpdated";

            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }

    }
}
