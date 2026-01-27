namespace CorrectBonus.Entities.System
{
    public class SystemSeedHistory
    {
        public int Id { get; set; }
        public string Version { get; set; } = null!;
        public DateTime AppliedAt { get; set; }
    }
}
