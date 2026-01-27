namespace CorrectBonus.Models.Regions
{
    public class RegionFormViewModel
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public int RegionTypeId { get; set; }

        public int? ParentRegionId { get; set; }

        // Manager
        public int? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerErpCode { get; set; }

        // Coefficient
        public bool HasCoefficient { get; set; }
        public decimal? Coefficient { get; set; }

        // ERP
        public string? ErpCode { get; set; }

        // Targets (READONLY – şimdilik)
        public decimal? TargetAmount { get; set; }
        public decimal? ActualAmount { get; set; }

        public bool IsActive { get; set; }
    }
}