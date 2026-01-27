using CorrectBonus.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.Authorization
{
    public class User : ITenantEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }

        public bool IsActive { get; set; }

        public int? TenantId { get; set; }

        // 🔥 KRİTİK KISIM
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]   // ⬅⬅⬅ BU SATIR
        public Role Role { get; set; } = null!;

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpireAt { get; set; }
        public DateTime? PasswordExpireAt { get; set; }

        [MaxLength(300)]
        public string? ProfileImagePath { get; set; }

        public string? PreferredLanguage { get; set; }
        public string? Phone { get; set; }
    }
}
