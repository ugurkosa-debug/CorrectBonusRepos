namespace CorrectBonus.Models.Regions
{
    public class RegionTreeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string RegionType { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<RegionTreeItem> Children { get; set; } = new();
    }
}
