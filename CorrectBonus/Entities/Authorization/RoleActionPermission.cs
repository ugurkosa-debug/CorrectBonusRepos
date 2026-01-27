using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.Authorization
{
    public class RoleActionPermission
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        public int PermissionId { get; set; }

        public ActionPermission Action { get; set; }

        // ===============================
        // NAVIGATION (eski yapı, dursun)
        // ===============================
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;
    }
}
