namespace CorrectBonus.Models.Regions
{
    public class RegionImportResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
