using CorrectBonus.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.Authorization
{
    [Table("Roles")]
    public class Role : ITenantEntity
    {
        [Key]
        public int Id { get; set; }   // ✅ DB'de Roles.Id

        public int? TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<RoleActionPermission> RoleActionPermissions { get; set; } = new List<RoleActionPermission>();
    }
}
