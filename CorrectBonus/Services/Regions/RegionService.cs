using CorrectBonus.Data;
using CorrectBonus.Entities.Regions;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.Regions
{
    public class RegionService
    {
        private readonly ApplicationDbContext _db;

        public RegionService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ===============================
        // LEVEL CALCULATION
        // ===============================
        public async Task<int> GetLevelAsync(int? parentId)
        {
            if (parentId == null)
                return 1;

            var parent = await _db.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == parentId);

            if (parent == null)
                return 1;

            return await GetLevelAsync(parent.ParentRegionId) + 1;
        }

        // ===============================
        // MAX LEVEL VALIDATION
        // ===============================
        public async Task<bool> ValidateMaxLevelAsync(
            int regionTypeId,
            int? parentRegionId)
        {
            var regionType = await _db.RegionTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == regionTypeId);

            if (regionType == null)
                return false;

            // 0 = limitsiz
            if (regionType.MaxLevel == 0)
                return true;

            var level = await GetLevelAsync(parentRegionId);
            return level <= regionType.MaxLevel;
        }
        // ===============================
        // GET ALL CHILDREN
        // ===============================
        private List<Region> GetAllChildren(
            List<Region> all,
            int parentId)
        {
            var children = all.Where(x => x.ParentRegionId == parentId).ToList();
            var result = new List<Region>(children);

            foreach (var child in children)
                result.AddRange(GetAllChildren(all, child.Id));

            return result;
        }

        // ===============================
        // DEACTIVATE WITH CHILDREN
        // ===============================
        public async Task<List<Region>> DeactivateWithChildrenAsync(int regionId)
        {
            var all = await _db.Regions.ToListAsync();

            var target = all.First(x => x.Id == regionId);
            var children = GetAllChildren(all, regionId);

            target.IsActive = false;
            foreach (var c in children)
                c.IsActive = false;

            await _db.SaveChangesAsync();
            return children;
        }

    }
}
