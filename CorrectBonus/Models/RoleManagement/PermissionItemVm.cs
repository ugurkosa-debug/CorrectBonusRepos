namespace CorrectBonus.Models.RoleManagement
{
    public class PermissionItemVm
    {
        public int PermissionId { get; set; }   // ✅ EKLENDİ

        public string Code { get; set; } = null!;
        public string Module { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
