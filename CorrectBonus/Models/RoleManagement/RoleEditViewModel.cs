using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.RoleManagement
{
    public class RoleEditViewModel
    {
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = null!;

        public bool IsActive { get; set; }

        // Tüm yetkiler (checkbox listesi)
        public List<PermissionItemVm> AllPermissions { get; set; }
            = new();

        // Seçili yetkiler (PermissionId)
        public List<int> SelectedPermissionIds { get; set; }
            = new();
    }
}
