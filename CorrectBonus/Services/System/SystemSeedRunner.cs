using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using CorrectBonus.Services.System.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CorrectBonus.Services.System
{
    public class SystemSeedRunner
    {
        private readonly ApplicationDbContext _db;
        private readonly IEnumerable<ISystemSeed> _seeds;

        public SystemSeedRunner(
            ApplicationDbContext db,
            IEnumerable<ISystemSeed> seeds)
        {
            _db = db;
            _seeds = seeds;
        }

        public async Task RunAsync()
        {
            // 🔥 MIGRATION GARANTİ (installer / runtime safe)
            _db.Database.Migrate();

            // 🔥 TABLO VAR MI KONTROL (EKSTRA GÜVENLİK)
            var creator = _db.Database.GetService<IRelationalDatabaseCreator>();
            if (!creator.Exists())
            {
                await creator.CreateAsync();
            }

            var appliedVersions = new List<string>();

            try
            {
                appliedVersions = await _db.SystemSeedHistories
                    .Select(x => x.Version)
                    .ToListAsync();
            }
            catch
            {
                // 🔥 TABLO YOKSA: HİÇ SEED ÇALIŞMAMIŞ KABUL ET
                appliedVersions = new List<string>();
            }

            foreach (var seed in _seeds.OrderBy(s => s.Version))
            {
                if (appliedVersions.Contains(seed.Version))
                    continue;

                await seed.ApplyAsync(_db);

                _db.SystemSeedHistories.Add(new SystemSeedHistory
                {
                    Version = seed.Version,
                    AppliedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
            }
        }

    }
}
