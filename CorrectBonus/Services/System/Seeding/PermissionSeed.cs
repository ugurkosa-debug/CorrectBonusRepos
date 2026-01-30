using CorrectBonus.Data;
using CorrectBonus.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.System.Seeding
{
    public static class PermissionSeed
    {
        public static void Seed(ApplicationDbContext db)
        {
            var permissions = new List<Permission>
            {
                // ================= USERS =================
                New("USERS_VIEW",    "Kullanıcılar", "Users", "Kullanıcı Listeleme", "View Users"),
                New("USERS_CREATE",  "Kullanıcılar", "Users", "Kullanıcı Oluşturma", "Create User"),
                New("USERS_EDIT",    "Kullanıcılar", "Users", "Kullanıcı Düzenleme", "Edit User"),
                New("USERS_DELETE",  "Kullanıcılar", "Users", "Kullanıcı Silme", "Delete User"),
                New("USERS_STATUS",  "Kullanıcılar", "Users", "Kullanıcı Durum", "Change User Status"),

                // ================= ROLES =================
                New("ROLES_VIEW",   "Roller", "Roles", "Rol Listeleme", "View Roles"),
                New("ROLES_CREATE", "Roller", "Roles", "Rol Oluşturma", "Create Role"),
                New("ROLES_EDIT",   "Roller", "Roles", "Rol Düzenleme", "Edit Role"),
                New("ROLES_DELETE", "Roller", "Roles", "Rol Silme", "Delete Role"),

                // ================= REGIONS =================
                New("REGIONS_VIEW",   "Bölgeler", "Regions", "Bölge Listeleme", "View Regions"),
                New("REGIONS_CREATE", "Bölgeler", "Regions", "Bölge Oluşturma", "Create Region"),
                New("REGIONS_EDIT",   "Bölgeler", "Regions", "Bölge Düzenleme", "Edit Region"),
                New("REGIONS_DELETE", "Bölgeler", "Regions", "Bölge Silme", "Delete Region"),
                New("REGIONS_STATUS", "Bölgeler", "Regions", "Bölge Durum", "Change Region Status"),

                // ================= REGION TYPES =================
                New("REGIONTYPES_VIEW",   "Bölge Tipleri", "Region Types", "Bölge Tipi Listeleme", "View Region Types"),
                New("REGIONTYPES_CREATE", "Bölge Tipleri", "Region Types", "Bölge Tipi Oluşturma", "Create Region Type"),
                New("REGIONTYPES_EDIT",   "Bölge Tipleri", "Region Types", "Bölge Tipi Düzenleme", "Edit Region Type"),

                // ================= SYSTEM SETTINGS =================
                New("SYSTEM_SETTINGS_VIEW", "Sistem Ayarları", "System Settings", "Ayarları Görüntüleme", "View Settings"),
                New("SYSTEM_SETTINGS_EDIT", "Sistem Ayarları", "System Settings", "Ayarları Düzenleme", "Edit Settings"),
                // ================= LOGS =================
                New("LOGS_VIEW",   "Sistem", "System", "Logları Görüntüleme", "View Logs"),
                New("LOGS_EXPORT", "Sistem", "System", "Logları Dışa Aktarma", "Export Logs"),
                // ================= TENANTS =================
                New("TENANTS_VIEW",   "Firmalar", "Tenants", "Firma Listeleme", "View Tenants"),
                New("TENANTS_CREATE", "Firmalar", "Tenants", "Firma Oluşturma", "Create Tenant"),
                New("TENANTS_EDIT",   "Firmalar", "Tenants", "Firma Düzenleme", "Edit Tenant"),
                New("TENANTS_STATUS", "Firmalar", "Tenants", "Firma Durum", "Change Tenant Status"),
                // ================= MENU (PAGE PERMISSIONS) =================
                New("SYSTEM_MENU_VIEW", "Sistem", "System", "Sistem Menüsü", "System Menu"),
                New("SYSTEM_SETTINGS_MENU_VIEW", "Sistem", "System", "Sistem Ayarları Menüsü", "System Settings Menu"),
                New("LOGS_MENU_VIEW", "Sistem", "System", "Loglar Menüsü", "Logs Menu"),

            };

            foreach (var p in permissions)
            {
                var existing = db.Permissions
                    .IgnoreQueryFilters()
                    .FirstOrDefault(x => x.Code == p.Code);

                if (existing == null)
                {
                    db.Permissions.Add(p);
                }
                else
                {
                    // 🔒 NULL GEÇİŞ YOK
                    existing.NameTr = p.NameTr;
                    existing.NameEn = p.NameEn;
                    existing.ModuleTr = p.ModuleTr;
                    existing.ModuleEn = p.ModuleEn;
                    existing.Type = p.Type;
                    existing.IsActive = true;
                }
            }

            db.SaveChanges();
        }

        private static Permission New(
            string code,
            string moduleTr,
            string moduleEn,
            string nameTr,
            string nameEn)
        {
            return new Permission
            {
                Code = code,
                ModuleTr = moduleTr,
                ModuleEn = moduleEn,
                NameTr = nameTr,
                NameEn = nameEn,
                Type = PermissionTypes.Page,
                IsActive = true
            };
        }
    }
}
