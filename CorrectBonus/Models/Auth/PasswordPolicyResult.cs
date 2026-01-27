namespace CorrectBonus.Models.Auth
{
    public class PasswordPolicyResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
    }
}
