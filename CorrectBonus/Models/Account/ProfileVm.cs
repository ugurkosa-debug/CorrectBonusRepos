using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.Account
{
    public class ProfileVm
    {
        [Required]
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();

        public string? ProfileImagePath { get; set; }
        public string? PreferredLanguage { get; set; }

    }
}
