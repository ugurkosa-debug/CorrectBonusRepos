using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.Auth
{
    public class ForgotPasswordVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string? Language { get; set; }
    }
}
