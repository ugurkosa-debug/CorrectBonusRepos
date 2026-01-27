using CorrectBonus.Entities.Authorization;
using CorrectBonus.Models.Auth;


namespace CorrectBonus.Services.Auth
{
    public interface IPasswordPolicyService
    {
        Task<PasswordPolicyResult> ValidateAsync(
            string password,
            User? user = null);

        DateTime CalculateExpireDate();
    }
}
