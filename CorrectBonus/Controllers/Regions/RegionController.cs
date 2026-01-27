using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Entities.Regions;
using CorrectBonus.Models.Regions;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers.Regions
{
    [Authorize]
    public class RegionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;
        private readonly RegionService _regionService;
        private readonly ILogService _logService;

        public RegionController(
            ApplicationDbContext db,
            CurrentUserContext currentUser,
            RegionService regionService,
            ILogService logService)
        {
            _db = db;
            _currentUser = currentUser;
            _regionService = regionService;
            _logService = logService;
        }

        // ===============================
        // TREE (AJAX)
        // ===============================
        [HttpGet]
        [RequirePermission("REGIONS_VIEW")]
        public async Task<IActionResult> Tree()
        {
            var regions = await _db.Regions
                .Include(r => r.RegionType)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_RegionTree", BuildTree(regions, null));
        }

        // ===============================
        // LIST (PAGE)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Regions.View)]
        public async Task<IActionResult> Index(string? search, string status = "all")
        {
            var query = _db.Regions
                .Include(r => r.RegionType)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.Name.Contains(search) ||
                    r.RegionType.Name.Contains(search));
            }

            query = status switch
            {
                "active" => query.Where(r => r.IsActive),
                "passive" => query.Where(r => !r.IsActive),
                _ => query
            };

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(await query.OrderBy(r => r.Name).ToListAsync());
        }

        // ===============================
        // CREATE (GET)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Regions.Create)]
        public IActionResult Create(int? parentId)
        {
            LoadRegionTypes();
            LoadParentRegions();
            LoadManagers();

            return View(new RegionFormViewModel
            {
                ParentRegionId = parentId,
                IsActive = true
            });
        }

        // ===============================
        // CREATE (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Regions.Create)]
        public async Task<IActionResult> Create(RegionFormViewModel model)
        {
            if (!await _regionService.ValidateMaxLevelAsync(
                model.RegionTypeId, model.ParentRegionId))
            {
                ModelState.AddModelError("", "Bu bölge tipi için izin verilen kat sayısı aşıldı.");
            }

            if (!ModelState.IsValid)
            {
                LoadRegionTypes();
                LoadParentRegions();
                LoadManagers();
                return View(model);
            }

            var entity = new Region
            {
                Name = model.Name,
                RegionTypeId = model.RegionTypeId,
                ParentRegionId = model.ParentRegionId,
                ManagerUserId = model.ManagerUserId,
                ManagerErpCode = model.ManagerErpCode,
                HasCoefficient = model.HasCoefficient,
                Coefficient = model.HasCoefficient ? model.Coefficient : null,
                ErpCode = model.ErpCode,
                TenantId = _currentUser.TenantId,
                IsActive = true
            };

            _db.Regions.Add(entity);
            await _db.SaveChangesAsync();

            await _logService.InfoAsync(
                "REGIONS_CREATE",
                $"Region created: {entity.Name}",
                _currentUser.Email);

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // EDIT (GET)
        // ===============================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Regions.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.Regions.FindAsync(id);
            if (entity == null)
                return NotFound();

            LoadRegionTypes();
            LoadParentRegions(id);

            return View(new RegionFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                RegionTypeId = entity.RegionTypeId,
                ParentRegionId = entity.ParentRegionId,
                ManagerErpCode = entity.ManagerErpCode,
                HasCoefficient = entity.HasCoefficient,
                Coefficient = entity.Coefficient,
                ErpCode = entity.ErpCode,
                IsActive = entity.IsActive
            });
        }

        // ===============================
        // EDIT (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Regions.Edit)]
        public async Task<IActionResult> Edit(RegionFormViewModel model)
        {
            if (!await _regionService.ValidateMaxLevelAsync(
                model.RegionTypeId, model.ParentRegionId))
            {
                ModelState.AddModelError("", "Bu bölge tipi için izin verilen kat sayısı aşıldı.");
            }

            if (!ModelState.IsValid)
            {
                LoadRegionTypes();
                LoadParentRegions(model.Id);
                return View(model);
            }

            var entity = await _db.Regions.FindAsync(model.Id);
            if (entity == null)
                return NotFound();

            entity.Name = model.Name;
            entity.RegionTypeId = model.RegionTypeId;
            entity.ParentRegionId = model.ParentRegionId;
            entity.ManagerErpCode = model.ManagerErpCode;
            entity.HasCoefficient = model.HasCoefficient;
            entity.Coefficient = model.HasCoefficient ? model.Coefficient : null;
            entity.ErpCode = model.ErpCode;

            await _db.SaveChangesAsync();

            await _logService.InfoAsync(
                "REGIONS_EDIT",
                $"Region updated: {entity.Name}",
                _currentUser.Email);

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // STATUS
        // ===============================
        [HttpPost]
        [RequirePermission(PermissionRegistry.Regions.Status)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var region = await _db.Regions.FindAsync(id);
            if (region == null)
                return NotFound();

            if (region.IsActive)
                await _regionService.DeactivateWithChildrenAsync(id);
            else
            {
                region.IsActive = true;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETE
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Regions.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _db.Regions.AnyAsync(x => x.ParentRegionId == id))
            {
                TempData["DeleteError"] = "Region.HasChildren";
                return RedirectToAction(nameof(Index));
            }

            var entity = await _db.Regions.FindAsync(id);
            if (entity == null)
                return NotFound();

            _db.Regions.Remove(entity);
            await _db.SaveChangesAsync();

            await _logService.InfoAsync(
                "REGIONS_DELETE",
                $"Region deleted: {entity.Name}",
                _currentUser.Email);

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // HELPERS
        // ===============================
        private List<RegionTreeItem> BuildTree(List<Region> regions, int? parentId)
        {
            return regions
                .Where(r => r.ParentRegionId == parentId)
                .OrderBy(r => r.Name)
                .Select(r => new RegionTreeItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    RegionType = r.RegionType.Name,
                    IsActive = r.IsActive,
                    Children = BuildTree(regions, r.Id)
                })
                .ToList();
        }

        private void LoadRegionTypes()
        {
            ViewBag.RegionTypes = _db.RegionTypes
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList();
        }

        private void LoadParentRegions(int? excludeId = null)
        {
            ViewBag.ParentRegions = _db.Regions
                .AsNoTracking()
                .Where(r => excludeId == null || r.Id != excludeId)
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToList();
        }

        private void LoadManagers()
        {
            ViewBag.Managers = _db.Users
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.FullName
                })
                .ToList();
        }
    }
}
