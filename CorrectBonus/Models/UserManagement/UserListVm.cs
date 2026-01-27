namespace CorrectBonus.Models.UserManagement
{
    public class UserListVm
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
