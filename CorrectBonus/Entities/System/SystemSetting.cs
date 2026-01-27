using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorrectBonus.Entities.System
{
    [Table("SystemSettings")]
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Group { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string SettingKey { get; set; } = null!; // 🔥 ESKİ: Key

        [MaxLength(500)]
        public string? Value { get; set; }

        [MaxLength(500)]
        public string? DefaultValue { get; set; }

        [Required]
        public string DescriptionTr { get; set; } = null!;

        [Required]
        public string DescriptionEn { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
    }
}
