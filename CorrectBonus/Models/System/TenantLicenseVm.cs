using System.ComponentModel.DataAnnotations;

namespace CorrectBonus.Models.System
{
    public class TenantLicenseVm
    {
        public int LicenseId { get; set; }      // 🔥 EKLENDİ
        public int TenantId { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
        public string PublicKey { get; set; } = "";
    }
}
