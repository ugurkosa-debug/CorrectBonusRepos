using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.System.Seeding
{
    public class MenuSeed
    {
        public async Task ApplyAsync(ApplicationDbContext db)
        {
            // ================= ROOT MENUS =================

            var userMgmt = await AddOrGetRootAsync(db, new Menu
            {
                Title = "Kullanıcı Yönetimi",
                Icon = "fa-users",
                Order = 10,
                PermissionCode = "USERS_MENU_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Users"
            });

            var regionMgmt = await AddOrGetRootAsync(db, new Menu
            {
                Title = "Bölge Yönetimi",
                Icon = "fa-map",
                Order = 20,
                PermissionCode = "REGIONS_MENU_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Regions"
            });

            var system = await AddOrGetRootAsync(db, new Menu
            {
                Title = "Sistem",
                Icon = "fa-cogs",
                Order = 100,
                PermissionCode = "SYSTEM_MENU_VIEW",
                IsActive = true,
                ResourceKey = "Menu.System"
            });

            // 🔴 ROOT'ları kaydet → ID'ler gelsin
            await db.SaveChangesAsync();

            // ================= CHILD MENUS =================

            await AddOrGetChildAsync(db, new Menu
            {
                Title = "Kullanıcılar",
                Controller = "Users",
                Action = "Index",
                Icon = "fa-user",
                Order = 1,
                ParentId = userMgmt.Id,
                PermissionCode = "USERS_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Users.List"
            });

            await AddOrGetChildAsync(db, new Menu
            {
                Title = "Roller",
                Controller = "Roles",
                Action = "Index",
                Icon = "fa-key",
                Order = 2,
                ParentId = userMgmt.Id,
                PermissionCode = "ROLES_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Roles"
            });

            // Bölgeler
            await AddOrGetChildAsync(db, new Menu
            {
                Title = "Bölgeler",
                Controller = "Region",
                Action = "Index",
                Icon = "fa-map-marker",
                Order = 1,
                ParentId = regionMgmt.Id,
                PermissionCode = "REGIONS_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Regions.List"
            });

            // Bölge Tipleri
            await AddOrGetChildAsync(db, new Menu
            {
                Title = "Bölge Tipleri",
                Controller = "RegionType",
                Action = "Index",
                Icon = "fa-tags",
                Order = 2,
                ParentId = regionMgmt.Id,
                PermissionCode = "REGIONTYPES_VIEW",
                IsActive = true,
                ResourceKey = "Menu.RegionTypes"
            });


            await AddOrGetChildAsync(db, new Menu
            {
                Title = "Firmalar",
                Controller = "Tenants",
                Action = "Index",
                Icon = "fa-building",
                Order = 3,
                ParentId = system.Id,
                PermissionCode = "TENANTS_VIEW",
                IsActive = true,
                ResourceKey = "Menu.Tenants.List"
            });


            await AddOrGetChildAsync(db, showSystemSettings(system));
            await AddOrGetChildAsync(db, showLogs(system));

            await db.SaveChangesAsync();
        }

        // ================= HELPERS =================

        private Menu showSystemSettings(Menu parent) => new Menu
        {
            Title = "Sistem Ayarları",
            Controller = "SystemSettings",
            Action = "Index",
            Icon = "fa-sliders-h",
            Order = 1,
            ParentId = parent.Id,
            PermissionCode = "SYSTEM_SETTINGS_VIEW",
            IsActive = true,
            ResourceKey = "Menu.SystemSettings"
        };

        private Menu showLogs(Menu parent) => new Menu
        {
            Title = "Loglar",
            Controller = "Logs",
            Action = "Index",
            Icon = "fa-list",
            Order = 2,
            ParentId = parent.Id,
            PermissionCode = "LOGS_VIEW",
            IsActive = true,
            ResourceKey = "Menu.Logs"
        };

        private async Task<Menu> AddOrGetRootAsync(ApplicationDbContext db, Menu menu)
        {
            var existing = await db.Menus
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x =>
                    x.ParentId == null &&
                    x.PermissionCode == menu.PermissionCode);

            if (existing != null)
            {
                Update(existing, menu);
                return existing;
            }

            db.Menus.Add(menu);
            return menu;
        }

        private async Task<Menu> AddOrGetChildAsync(ApplicationDbContext db, Menu menu)
        {
            var existing = await db.Menus
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x =>
                    x.ParentId == menu.ParentId &&
                    x.PermissionCode == menu.PermissionCode);

            if (existing != null)
            {
                Update(existing, menu);
                return existing;
            }

            db.Menus.Add(menu);
            return menu;
        }

        private void Update(Menu target, Menu source)
        {
            target.Title = source.Title;
            target.Icon = source.Icon;
            target.Order = source.Order;
            target.Controller = source.Controller;
            target.Action = source.Action;
            target.PermissionCode = source.PermissionCode;
            target.ResourceKey = source.ResourceKey;
            target.IsActive = true;
        }
    }
}
