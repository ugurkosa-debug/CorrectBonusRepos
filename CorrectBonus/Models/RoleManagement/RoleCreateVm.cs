using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.RoleManagement
{
    public class RoleCreateVm
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}
