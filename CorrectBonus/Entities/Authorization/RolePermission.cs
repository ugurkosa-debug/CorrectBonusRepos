using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.Authorization
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        // ===============================
        // COMPOSITE KEY (DB GERÇEĞİ)
        // ===============================
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // ===============================
        // NAVIGATION
        // ===============================
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;
    }
}
