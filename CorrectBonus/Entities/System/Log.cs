using CorrectBonus.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.System
{
    [Table("Logs")]
    public class Log : ITenantEntity
    {
        [Key]
        public long Id { get; set; }

        // 🔑 Nullable – login öncesi / system loglar
        public int? TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = null!;

        [MaxLength(200)]
        public string? UserEmail { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        public string? Exception { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
