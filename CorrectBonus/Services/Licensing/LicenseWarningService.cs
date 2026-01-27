using CorrectBonus.Data;
using CorrectBonus.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.Licensing
{
    public class LicenseWarningService
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;

        public LicenseWarningService(
            ApplicationDbContext db,
            CurrentUserContext currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<LicenseWarningResult?> GetWarningAsync()
        {
            if (_currentUser.IsOwner || _currentUser.TenantId == null)
                return null;

            var license = await _db.TenantLicenses
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == _currentUser.TenantId &&
                    x.Status == "Active")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (license == null)
                return null;

            var daysLeft = (license.ExpireAt.Date - DateTime.UtcNow.Date).Days;

            if (daysLeft <= 0)
                return null;

            if (daysLeft <= 7)
            {
                return new LicenseWarningResult
                {
                    DaysLeft = daysLeft,
                    Level = LicenseWarningLevel.Critical
                };
            }

            if (daysLeft <= 30)
            {
                return new LicenseWarningResult
                {
                    DaysLeft = daysLeft,
                    Level = LicenseWarningLevel.Warning
                };
            }

            return null;
        }
    }

    public class LicenseWarningResult
    {
        public int DaysLeft { get; set; }
        public LicenseWarningLevel Level { get; set; }
    }

    public enum LicenseWarningLevel
    {
        Warning,
        Critical
    }
}
