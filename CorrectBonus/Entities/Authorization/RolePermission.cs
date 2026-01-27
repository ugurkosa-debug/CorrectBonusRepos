using CorrectBonus.Entities.Authorization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.Authorization
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        // ===============================
        // FK: ROLE
        // ===============================
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;

        // ===============================
        // FK: PERMISSION
        // ===============================
        public int PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;
    }
}
