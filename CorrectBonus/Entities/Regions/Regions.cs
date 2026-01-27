using CorrectBonus.Entities.Authorization;
using CorrectBonus.Entities.Common;

namespace CorrectBonus.Entities.Regions
{
    public class Region : ITenantEntity
    {
        public int Id { get; set; }

        // ===============================
        // 🌳 HIERARCHY
        // ===============================
        public int? ParentRegionId { get; set; }
        public Region? ParentRegion { get; set; }
        public ICollection<Region> Children { get; set; } = new List<Region>();

        // ===============================
        // 📌 BASIC INFO
        // ===============================
        public string Name { get; set; } = null!;

        public int RegionTypeId { get; set; }
        public RegionType RegionType { get; set; } = null!;

        // ===============================
        // 👤 MANAGER
        // ===============================
        public int? ManagerUserId { get; set; }
        public User? ManagerUser { get; set; }
        public string? ManagerErpCode { get; set; }

        // ===============================
        // 📊 COEFFICIENT
        // ===============================
        public bool HasCoefficient { get; set; }
        public decimal? Coefficient { get; set; }

        // ===============================
        // 🔗 ERP
        // ===============================
        public string? ErpCode { get; set; }

        // ===============================
        // 🎯 TARGETS (FUTURE)
        // ===============================
        public decimal? TargetValue { get; set; }

        // ===============================
        // ⚙ SYSTEM
        // ===============================
        public bool IsActive { get; set; } = true;

        // ===============================
        // 🌍 TENANT
        // ===============================
        public int? TenantId { get; set; }
    }
}
