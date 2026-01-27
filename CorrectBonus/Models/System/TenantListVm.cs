namespace CorrectBonus.Models.System
{
    public class TenantListVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string LicenseStatus { get; set; } = "None";
        public List<string> ActiveModules { get; set; } = new();
        public string Modules { get; set; } = "";

    }
}
