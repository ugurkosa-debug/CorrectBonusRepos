using CorrectBonus.Entities.Authorization;
using CorrectBonus.Entities.System;

namespace CorrectBonus.Models.MenuManagement
{
    public class MenuEditVm
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Controller { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Icon { get; set; }

        public int Order { get; set; }
        public int? ParentId { get; set; }

        public string PermissionCode { get; set; } = null!;
        public bool IsActive { get; set; }

        public List<Menu> ParentMenus { get; set; } = new();
        public List<Permission> Permissions { get; set; } = new();
    }

}
