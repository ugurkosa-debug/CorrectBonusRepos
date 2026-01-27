namespace CorrectBonus.Entities.Authorization
{
    public class Permission
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;
        public string NameTr { get; set; } = null!;
        public string NameEn { get; set; } = null!;

        public string ModuleTr { get; set; } = null!;
        public string ModuleEn { get; set; } = null!;

        public string Type { get; set; } = PermissionTypes.Page; // Page | Action

        public bool IsActive { get; set; } = true;

        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }

    public static class PermissionTypes
    {
        public const string Page = "Page";
        public const string Action = "Action";
    }
}
