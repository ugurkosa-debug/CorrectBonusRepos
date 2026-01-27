namespace CorrectBonus.Authorization
{
    public static class PermissionRegistry
    {
        // ================= USERS =================
        public static class Users
        {
            public const string View = "USERS_VIEW";
            public const string Create = "USERS_CREATE";
            public const string Edit = "USERS_EDIT";
            public const string Delete = "USERS_DELETE";
            public const string Status = "USERS_STATUS";
        }

        public static class Roles
        {
            public const string View = "ROLES_VIEW";
            public const string Create = "ROLES_CREATE";
            public const string Edit = "ROLES_EDIT";
            public const string Delete = "ROLES_DELETE";
        }

        public static class Regions
        {
            public const string View = "REGIONS_VIEW";
            public const string Create = "REGIONS_CREATE";
            public const string Edit = "REGIONS_EDIT";
            public const string Delete = "REGIONS_DELETE";
            public const string Status = "REGIONS_STATUS";
        }

        public static class RegionTypes
        {
            public const string View = "REGIONTYPES_VIEW";
            public const string Create = "REGIONTYPES_CREATE";
            public const string Edit = "REGIONTYPES_EDIT";
        }

        public static class Logs
        {
            public const string View = "LOGS_VIEW";
            public const string Export = "LOGS_EXPORT";
        }

        public static class System
        {
            public const string SettingsView = "SYSTEM_SETTINGS_VIEW";
            public const string SettingsEdit = "SYSTEM_SETTINGS_EDIT";
        }

        // ================= TENANTS =================
        public static class Tenants
        {
            public const string View = "TENANTS_VIEW";
            public const string Create = "TENANTS_CREATE";
            public const string Edit = "TENANTS_EDIT";
            public const string Status = "TENANTS_STATUS";
        }
    }
}
