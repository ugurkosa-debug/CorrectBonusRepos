using CorrectBonus.Data;
using CorrectBonus.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.Licensing
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrentUserContext _currentUser;
        private readonly LicenseValidator _validator;

        public LicenseService(
            ApplicationDbContext context,
            CurrentUserContext currentUser,
            LicenseValidator validator)
        {
            _context = context;
            _currentUser = currentUser;
            _validator = validator;
        }

        public async Task<LicenseState> GetCurrentTenantLicenseStateAsync()
        {
            // 🔑 Default tenant
            if (_currentUser.TenantId <= 0)
                return LicenseState.Active;

            if (_currentUser.IsOwner)
                return LicenseState.Active;

            var license = await _context.TenantLicenses
                .Where(x => x.TenantId == _currentUser.TenantId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (license == null)
                return LicenseState.Invalid;

            // 🔴 MANUEL KAPATMA (EN ÜST ÖNCELİK)
            if (license.Status != "Active")
                return LicenseState.Invalid;

            // ⏰ Süre dolmuş mu?
            if (license.ExpireAt <= DateTime.UtcNow)
                return LicenseState.Expired;

            // 🔐 İmza doğrulama (opsiyonel)
            var payload = _validator.Validate(license.LicenseKey);
            if (payload == null)
                return LicenseState.Invalid;

            return LicenseState.Active;
        }
    }
}
