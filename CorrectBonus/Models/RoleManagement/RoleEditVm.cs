using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.RoleManagement
{
    public class RoleEditVm
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }

        public List<PermissionCheckboxVm> Permissions { get; set; } = new();
    }
}
