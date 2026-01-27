public class RegionTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<RegionTreeDto> Children { get; set; } = new();
}
