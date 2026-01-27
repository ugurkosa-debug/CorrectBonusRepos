using CorrectBonus.Attributes;
using CorrectBonus.Data;
using CorrectBonus.Entities.Authorization;
using CorrectBonus.Models.RoleManagement;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Logs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logger;
        private readonly CurrentUserContext _currentUser;

        public RolesController(
            ApplicationDbContext context,
            ILogService logger,
            CurrentUserContext currentUser)
        {
            _context = context;
            _logger = logger;
            _currentUser = currentUser;
        }

        // ==================================================
        // INDEX (PAGE)
        // ==================================================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Roles.View)]
        public async Task<IActionResult> Index()
        {
            var query = _context.Roles.AsQueryable();

            if (_currentUser.IsOwner)
                query = query.IgnoreQueryFilters();
            else
                query = query.Where(r => r.TenantId != null);

            var roles = await query
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(roles);
        }

        // ==================================================
        // CREATE (GET)
        // ==================================================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Roles.Create)]
        public IActionResult Create()
        {
            if (_currentUser.IsOwner)
                return Forbid();

            return View(new RoleCreateVm());
        }

        // ==================================================
        // CREATE (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Roles.Create)]
        public async Task<IActionResult> Create(RoleCreateVm model)
        {
            if (_currentUser.IsOwner)
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var exists = await _context.Roles.AnyAsync(r =>
                r.TenantId == _currentUser.TenantId &&
                r.Name == model.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Bu rol adı zaten mevcut.");
                return View(model);
            }

            var role = new Role
            {
                Name = model.Name,
                IsActive = model.IsActive,
                TenantId = _currentUser.TenantId!.Value
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await _logger.InfoAsync(
                "ROLE_CREATED",
                $"Rol oluşturuldu: {role.Name}",
                User.Identity?.Name
            );

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // EDIT (GET)
        // ==================================================
        [HttpGet]
        [RequirePermission(PermissionRegistry.Roles.View)]
        public async Task<IActionResult> Edit(int id)
        {
            var roleQuery = _context.Roles
                .Include(r => r.RolePermissions)
                .AsQueryable();

            if (_currentUser.IsOwner)
                roleQuery = roleQuery.IgnoreQueryFilters();

            var role = await roleQuery.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return NotFound();

            if (!_currentUser.IsOwner && role.TenantId == null)
                return Forbid();

            var permissionsQuery = _context.Permissions.AsNoTracking();

            if (_currentUser.IsOwner)
                permissionsQuery = permissionsQuery.IgnoreQueryFilters();

            var allPermissions = await permissionsQuery
                .OrderBy(p => p.ModuleEn)
                .ThenBy(p => p.Code)
                .Select(p => new PermissionItemVm
                {
                    PermissionId = p.Id,
                    Code = p.Code,
                    Module = p.ModuleEn,
                    Name = p.NameEn,
                    Type = p.Type
                })
                .ToListAsync();

            var selectedPermissionIds = role.RolePermissions
                .Select(rp => rp.PermissionId)
                .ToList();

            var model = new RoleEditViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsActive = role.IsActive,
                AllPermissions = allPermissions,
                SelectedPermissionIds = selectedPermissionIds
            };

            return View(model);
        }

        // ==================================================
        // EDIT (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Roles.Edit)]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var roleQuery = _context.Roles
                .Include(r => r.RolePermissions)
                .AsQueryable();

            if (_currentUser.IsOwner)
                roleQuery = roleQuery.IgnoreQueryFilters();

            var role = await roleQuery.FirstOrDefaultAsync(r => r.Id == model.RoleId);
            if (role == null)
                return NotFound();

            if (!_currentUser.IsOwner && role.TenantId == null)
                return Forbid();

            role.Name = model.RoleName;
            role.IsActive = model.IsActive;

            role.RolePermissions.Clear();

            foreach (var permissionId in model.SelectedPermissionIds.Distinct())
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            await _context.SaveChangesAsync();

            await _logger.InfoAsync(
                "ROLE_UPDATED",
                $"Rol güncellendi: {role.Name}",
                User.Identity?.Name
            );

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // DELETE
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionRegistry.Roles.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var roleQuery = _context.Roles
                .Include(r => r.RolePermissions)
                .AsQueryable();

            if (_currentUser.IsOwner)
                roleQuery = roleQuery.IgnoreQueryFilters();

            var role = await roleQuery.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return NotFound();

            if (role.Name == "Admin")
            {
                TempData["Error"] = "Admin rolü silinemez.";
                return RedirectToAction(nameof(Index));
            }

            if (!_currentUser.IsOwner && role.TenantId == null)
                return Forbid();

            var hasUsers = await _context.Users.AnyAsync(u => u.RoleId == role.Id);
            if (hasUsers)
            {
                TempData["Error"] = "Bu role atanmış kullanıcılar bulunduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            role.RolePermissions.Clear();
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            await _logger.InfoAsync(
                "ROLE_DELETED",
                $"Rol silindi: {role.Name}",
                User.Identity?.Name
            );

            TempData["Success"] = "Rol başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}
