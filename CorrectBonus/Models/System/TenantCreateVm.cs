using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.System
{
    public class TenantCreateVm
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string AdminFullName { get; set; } = null!;
    }
}