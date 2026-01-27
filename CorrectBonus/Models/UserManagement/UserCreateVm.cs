using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.UserManagement
{
    public class UserCreateVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;
        public List<SelectListItem> Roles { get; set; } = new();
    }
}