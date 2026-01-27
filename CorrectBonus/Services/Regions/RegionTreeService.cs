using CorrectBonus.Entities.Regions;

namespace CorrectBonus.Services.Regions
{
    public class RegionTreeService
    {
        public List<RegionTreeDto> BuildTree(List<Region> regions, int? parentId)
        {
            return regions
                .Where(x => x.ParentRegionId == parentId)
                .Select(x => new RegionTreeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Children = BuildTree(regions, x.Id)
                })
                .ToList();
        }
    }
}
