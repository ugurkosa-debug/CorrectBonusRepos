using CorrectBonus.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CorrectBonus.Security
{
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ApplicationDbContext _db;

        public PermissionAuthorizationHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return;

            var hasPermission = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.Role != null)
                .SelectMany(u => u.Role!.RolePermissions)
                .Include(rp => rp.Permission)
                .AnyAsync(rp =>
                    rp.Permission != null &&
                    rp.Permission.Code == requirement.PermissionCode);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
