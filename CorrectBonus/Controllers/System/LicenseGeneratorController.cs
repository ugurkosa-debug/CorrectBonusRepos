using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Licensing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers.System
{
    [Authorize]
    public class LicenseGeneratorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrentUserContext _currentUser;
        private readonly LicenseGeneratorService _generator;

        public LicenseGeneratorController(
            ApplicationDbContext context,
            CurrentUserContext currentUser,
            LicenseGeneratorService generator)
        {
            _context = context;
            _currentUser = currentUser;
            _generator = generator;
        }

        // ==================================================
        // GENERATOR (GET)
        // ==================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (!_currentUser.IsOwner)
                return RedirectToAction("AccessDenied", "Auth");

            return View(new LicensePayload
            {
                Exp = DateTime.UtcNow.AddYears(1)
            });
        }

        // ==================================================
        // GENERATOR (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(
            string tenant,
            DateTime exp,
            string[] modules)
        {
            if (!_currentUser.IsOwner)
                return RedirectToAction("AccessDenied", "Auth");

            if (string.IsNullOrWhiteSpace(tenant))
            {
                ModelState.AddModelError("", "Tenant bilgisi zorunludur.");
                return View("Index", new LicensePayload { Exp = exp });
            }

            var payload = new LicensePayload
            {
                Tenant = tenant.Trim(),
                Exp = exp,
                Modules = modules ?? Array.Empty<string>()
            };

            // 🔐 Uzun, imzalı lisans
            var signedLicense = _generator.GenerateSignedLicense(payload);

            // 🔑 Kısa, kullanıcıya gösterilecek anahtar
            var publicKey = _generator.GeneratePublicKey();

            // Tenant bulunur
            var tenantEntity = await _context.Tenants
                .FirstOrDefaultAsync(x => x.Code == payload.Tenant);

            if (tenantEntity == null)
            {
                ModelState.AddModelError("", "Tenant bulunamadı.");
                return View("Index", payload);
            }

            // 🔴 Eski aktif lisansları pasif yap
            var oldLicenses = await _context.TenantLicenses
                .Where(x => x.TenantId == tenantEntity.Id && x.Status == "Active")
                .ToListAsync();

            foreach (var l in oldLicenses)
                l.Status = "Expired";

            // Yeni lisans
            var license = new TenantLicense
            {
                TenantId = tenantEntity.Id,
                LicenseKey = signedLicense, // 🔐
                PublicKey = publicKey,       // 🔑
                ExpireAt = payload.Exp,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.TenantLicenses.Add(license);
            await _context.SaveChangesAsync();

            // View’a kısa anahtar döner
            ViewBag.PublicKey = publicKey;

            return View("Index", payload);
        }
    }
}
