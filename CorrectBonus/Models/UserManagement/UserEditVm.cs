using Microsoft.AspNetCore.Mvc.Rendering;

namespace CorrectBonus.Models.UserManagement
{
    public class UserEditVm
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();
    }

}
