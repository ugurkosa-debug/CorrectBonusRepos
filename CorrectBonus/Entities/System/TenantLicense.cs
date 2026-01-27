namespace CorrectBonus.Entities.System
{
    public class TenantLicense
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        // 🔐 Uzun, imzalı lisans
        public string LicenseKey { get; set; } = null!;

        // 🔑 Kısa, kullanıcıya verilen anahtar
        public string PublicKey { get; set; } = null!;

        // Active / Expired / Invalid
        public string Status { get; set; } = "Active";

        public DateTime ExpireAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Notified30Days { get; set; }
        public bool Notified7Days { get; set; }
        public bool Notified1Day { get; set; }
    }
}
