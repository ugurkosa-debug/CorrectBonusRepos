namespace CorrectBonus.Models.Auth
{
    public class LoginVm
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        // ileride kullanılacak
        public string? Language { get; set; }

    };
}

