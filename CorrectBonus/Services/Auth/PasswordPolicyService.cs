using CorrectBonus.Data;
using CorrectBonus.Entities.Authorization;
using CorrectBonus.Models.Auth;
using CorrectBonus.Services.Logs;
using CorrectBonus.Services.System;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CorrectBonus.Services.Auth
{
    public class PasswordPolicyService : IPasswordPolicyService
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemSettingService _settings;
        private readonly ILogService _logger;

        public PasswordPolicyService(
            ApplicationDbContext context,
            SystemSettingService settings,
            ILogService logger)
        {
            _context = context;
            _settings = settings;
            _logger = logger;
        }

        public async Task<PasswordPolicyResult> ValidateAsync(
            string password,
            User? user = null)
        {
            var result = new PasswordPolicyResult();

            int minLength = _settings.GetInt("Password.MinLength", 8);
            bool upper = _settings.GetBool("Password.RequireUppercase");
            bool lower = _settings.GetBool("Password.RequireLowercase");
            bool digit = _settings.GetBool("Password.RequireDigit");
            bool special = _settings.GetBool("Password.RequireSpecial");
            int historyCount = _settings.GetInt("Password.HistoryCount", 0);

            if (password.Length < minLength)
                result.Errors.Add($"Şifre en az {minLength} karakter olmalıdır.");

            if (upper && !Regex.IsMatch(password, "[A-Z]"))
                result.Errors.Add("En az 1 büyük harf içermelidir.");

            if (lower && !Regex.IsMatch(password, "[a-z]"))
                result.Errors.Add("En az 1 küçük harf içermelidir.");

            if (digit && !Regex.IsMatch(password, "[0-9]"))
                result.Errors.Add("En az 1 rakam içermelidir.");

            if (special && !Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                result.Errors.Add("En az 1 özel karakter içermelidir.");

            // 🔴 PASSWORD HISTORY
            if (user != null && historyCount > 0)
            {
                var histories = await _context.UserPasswordHistories
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(historyCount)
                    .ToListAsync();

                foreach (var h in histories)
                {
                    if (Verify(password, h.PasswordHash, h.PasswordSalt))
                    {
                        result.Errors.Add("Son kullanılan şifreler tekrar kullanılamaz.");

                        await _logger.InfoAsync(
                            "PASSWORD_REUSE_BLOCKED",
                            $"UserId:{user.Id}",
                            user.Email);

                        break;
                    }
                }
            }

            if (!result.IsValid)
            {
                await _logger.InfoAsync(
                    "PASSWORD_POLICY_FAILED",
                    string.Join(" | ", result.Errors),
                    user?.Email);
            }

            return result;
        }

        public DateTime CalculateExpireDate()
        {
            int days = _settings.GetInt("Password.ExpireDays", 90);
            return DateTime.UtcNow.AddDays(days);
        }

        private static bool Verify(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);

            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(hash);
        }
    }
}
