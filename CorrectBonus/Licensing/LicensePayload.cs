namespace CorrectBonus.Services.Licensing
{
    public class LicensePayload
    {
        public string Tenant { get; set; } = null!;
        public DateTime Exp { get; set; }
        public string[] Modules { get; set; } = Array.Empty<string>();

        // Lisans metadata (imza dahil edilir!)
        public DateTime Issued { get; set; }
        public string Nonce { get; set; } = null!;
    }

}

