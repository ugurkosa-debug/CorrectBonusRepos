using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.SystemManagement
{
    public class SystemSettingEditVm
    {
        public int Id { get; set; }

        [Required]
        public string Group { get; set; } = null!;

        [Required]
        public string Key { get; set; } = null!;

        public string? Value { get; set; }

        public required string DescriptionTr { get; set; }
        public required string DescriptionEn { get; set; }

        public bool IsActive { get; set; }
    }
}
