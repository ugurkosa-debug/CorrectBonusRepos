using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Licensing;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.System
{
    public class MenuService
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;
        private readonly ILicenseService _licenseService;

        public MenuService(
            ApplicationDbContext db,
            CurrentUserContext currentUser,
            ILicenseService licenseService)
        {
            _db = db;
            _currentUser = currentUser;
            _licenseService = licenseService;
        }

        public List<Menu> GetUserMenus()
        {
            if (!_currentUser.IsOwner && _currentUser.RoleId == null)
                return new List<Menu>();

            var pagePermissions = _currentUser.IsOwner
                ? new HashSet<string>()
                : _db.RolePermissions
                    .AsNoTracking()
                    .Include(rp => rp.Permission)
                    .Where(rp =>
                        rp.RoleId == _currentUser.RoleId &&
                        rp.Permission.IsActive &&
                        rp.Permission.Code.EndsWith("_VIEW"))
                    .Select(rp => rp.Permission.Code)
                    .ToHashSet();

            var allMenus = _db.Menus
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToList();

            var lookup = allMenus.ToLookup(m => m.ParentId);
            foreach (var menu in allMenus)
                menu.Children = lookup[menu.Id].ToList();

            foreach (var menu in allMenus)
            {
                if (_currentUser.IsOwner)
                {
                    menu.IsAllowed = true;
                    continue;
                }

                if (menu.IsHostOnly)
                {
                    menu.IsAllowed = false;
                    continue;
                }

                menu.IsAllowed =
                    string.IsNullOrWhiteSpace(menu.PermissionCode) ||
                    pagePermissions.Contains(menu.PermissionCode);
            }

            foreach (var menu in allMenus)
            {
                if (menu.Children.Any())
                {
                    menu.Children = menu.Children
                        .Where(c => c.IsAllowed)
                        .OrderBy(c => c.Order)
                        .ToList();

                    menu.IsAllowed = menu.Children.Any();
                }
            }

            return allMenus
                .Where(m => m.ParentId == null && m.IsAllowed)
                .OrderBy(m => m.Order)
                .ToList();
        }
    }
}
