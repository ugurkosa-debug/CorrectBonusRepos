using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.System
{
    public class Menu
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Controller { get; set; } = null!;
        public string? Action { get; set; } = null!;
        public string? Icon { get; set; }

        public int Order { get; set; }

        public int? ParentId { get; set; }
        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = new List<Menu>();

        public string? PermissionCode { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public string? ResourceKey { get; set; }

        // 🔐 Runtime-only: MenuService tarafından set edilir
        [NotMapped]
        public bool IsAllowed { get; set; }
        public bool IsHostOnly { get; set; }

    }
}
