using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.System.Seeding
{
    public class MenuSeed : ISystemSeed
    {
        public string Version => "1.0.0-menu";

        public async Task ApplyAsync(ApplicationDbContext db)
        {
            // =========================
            // PARENT MENUS
            // =========================
            var systemMenu = await AddOrGetAsync(db,
                title: "System",
                resourceKey: "Menu.System",
                order: 100);

            var regionsMenu = await AddOrGetAsync(db,
                title: "Regions",
                resourceKey: "Menu.Regions",
                order: 20);

            // =========================
            // CHILD MENUS – USERS / ROLES
            // =========================
            await AddOrGetAsync(db,
                title: "Users",
                controller: "Users",
                action: "Index",
                resourceKey: "Menu.Users",
                permissionCode: "USERS_VIEW",
                order: 1);

            await AddOrGetAsync(db,
                title: "Roles",
                controller: "Roles",
                action: "Index",
                resourceKey: "Menu.Roles",
                permissionCode: "ROLES_VIEW",
                order: 2);

            // =========================
            // CHILD MENUS – REGIONS
            // =========================
            await AddOrGetAsync(db,
                title: "Regions",
                controller: "Region",
                action: "Index",
                resourceKey: "Menu.Regions",
                permissionCode: "REGIONS_VIEW",
                parent: regionsMenu,
                order: 1);

            await AddOrGetAsync(db,
                title: "Region Types",
                controller: "RegionType",
                action: "Index",
                resourceKey: "Menu.RegionTypes",
                permissionCode: "REGIONTYPES_VIEW",
                parent: regionsMenu,
                order: 2);

            // =========================
            // SYSTEM CHILDREN
            // =========================
            await AddOrGetAsync(db,
                title: "System Settings",
                controller: "SystemSettings",
                action: "Index",
                resourceKey: "Menu.SystemSettings",
                permissionCode: "SYSTEM_SETTINGS_VIEW",
                parent: systemMenu,
                order: 1);

            await AddOrGetAsync(db,
                title: "Logs",
                controller: "Logs",
                action: "Index",
                resourceKey: "Menu.Logs",
                permissionCode: "LOGS_VIEW",
                parent: systemMenu,
                order: 2);

            await db.SaveChangesAsync();
        }

        // ==================================================
        // HELPER
        // ==================================================
        private static async Task<Menu> AddOrGetAsync(
            ApplicationDbContext db,
            string title,
            string? controller = null,
            string? action = null,
            string? resourceKey = null,
            string? permissionCode = null,
            Menu? parent = null,
            int order = 0)
        {
            var existing = await db.Menus
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m =>
                    m.Controller == controller &&
                    m.Action == action &&
                    m.ParentId == parent!.Id);

            if (existing != null)
            {
                // 🔒 Güncelle – ama NULL geçme
                existing.Title = title;
                existing.ResourceKey = resourceKey ?? existing.ResourceKey;
                existing.PermissionCode = permissionCode;
                existing.Order = order;
                existing.IsActive = true;

                return existing;
            }

            var menu = new Menu
            {
                Title = title,
                Controller = controller,
                Action = action,
                ResourceKey = resourceKey,
                PermissionCode = permissionCode,
                ParentId = parent?.Id,
                Order = order,
                IsActive = true
            };

            db.Menus.Add(menu);
            await db.SaveChangesAsync();

            return menu;
        }
    }
}
