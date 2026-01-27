using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.Auth
{
    public class ResetPasswordVm
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
