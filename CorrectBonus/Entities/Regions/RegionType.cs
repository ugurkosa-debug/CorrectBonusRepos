using CorrectBonus.Entities.Common;

namespace CorrectBonus.Entities.Regions
{
    public class RegionType : ITenantEntity
    {
        public int Id { get; set; }

        // Görünen ad
        public string Name { get; set; } = null!;

        // Kaç seviye derinliğe izin verir (0 = limitsiz)
        public int MaxLevel { get; set; }

        // Açık / Kapalı
        public bool IsActive { get; set; } = true;

        // Tenant
        public int? TenantId { get; set; }
    }
}
