namespace CorrectBonus.Models.MenuManagement
{
    public class MenuListVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
        public bool IsActive { get; set; }
    }

}
