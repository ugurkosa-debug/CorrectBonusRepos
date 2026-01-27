using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.Account
{
    public class ChangePasswordVm
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }

}
