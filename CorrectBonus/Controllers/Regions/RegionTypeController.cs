using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Entities.Regions;
using CorrectBonus.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers.Regions
{
    [Authorize]
    public class RegionTypeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;

        public RegionTypeController(
            ApplicationDbContext db,
            CurrentUserContext currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        // ===============================
        // LIST (PAGE)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.RegionTypes.View)]
        public async Task<IActionResult> Index()
        {
            var list = await _db.RegionTypes
                .AsNoTracking()
                .Where(x => x.TenantId == _currentUser.TenantId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(list);
        }

        // ===============================
        // CREATE (GET)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.RegionTypes.Create)]
        public IActionResult Create()
        {
            return View(new RegionType
            {
                IsActive = true
            });
        }

        // ===============================
        // CREATE (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.RegionTypes.Create)]
        public async Task<IActionResult> Create(RegionType model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.TenantId = _currentUser.TenantId;
            model.IsActive = true;

            _db.RegionTypes.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // EDIT (GET)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.RegionTypes.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.RegionTypes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.TenantId == _currentUser.TenantId);

            if (entity == null)
                return NotFound();

            return View(entity);
        }

        // ===============================
        // EDIT (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.RegionTypes.Edit)]
        public async Task<IActionResult> Edit(RegionType model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = await _db.RegionTypes
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id &&
                    x.TenantId == _currentUser.TenantId);

            if (entity == null)
                return NotFound();

            entity.Name = model.Name;
            entity.MaxLevel = model.MaxLevel;
            entity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
