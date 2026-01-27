using CorrectBonus.Data;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Services.Auth
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserContext _currentUser;

        public PermissionService(ApplicationDbContext db, CurrentUserContext currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public bool Has(string permissionCode)
        {
            if (_currentUser.RoleId == null)
                return false;

            return _db.RolePermissions
                .Include(rp => rp.Permission)
                .Any(rp =>
                    rp.RoleId == _currentUser.RoleId &&
                    rp.Permission.Code == permissionCode &&
                    rp.Permission.IsActive);
        }
    }
}
