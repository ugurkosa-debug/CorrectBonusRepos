using CorrectBonus.Data;
using CorrectBonus.Services.Auth;
using CorrectBonus.Services.Licensing;
using Microsoft.EntityFrameworkCore;

namespace CorrectBonus.Middleware
{
    public class TenantLicenseMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantLicenseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
    HttpContext context,
    ApplicationDbContext db,
    CurrentUserContext currentUser,
    LicenseValidator validator)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 🔓 Bypass edilen yollar
            if (
                path.StartsWith("/install") ||
                path.StartsWith("/auth") ||
                path.StartsWith("/license") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib")
            )
            {
                await _next(context);
                return;
            }

            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            if (currentUser.IsOwner || currentUser.TenantId == null)
            {
                await _next(context);
                return;
            }

            var license = await db.TenantLicenses
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == currentUser.TenantId &&
                    x.Status == "Active")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (license == null)
            {
                context.Response.Redirect("/License/Required");
                return;
            }

            var payload = validator.Validate(license.LicenseKey);

            if (payload == null || payload.Exp < DateTime.UtcNow)
            {
                context.Response.Redirect("/License/Expired");
                return;
            }

            await _next(context);
        }

    }
}
