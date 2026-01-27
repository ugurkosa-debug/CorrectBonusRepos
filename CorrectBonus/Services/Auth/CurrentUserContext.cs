using System.Security.Claims;

namespace CorrectBonus.Services.Auth
{
    public class CurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId =>
            TryGetIntClaim(ClaimTypes.NameIdentifier);

        public int? TenantId =>
            TryGetIntClaim("TenantId");

        public int? RoleId =>
            TryGetIntClaim("RoleId");

        public string? Email =>
            GetClaim(ClaimTypes.Name);

        /// <summary>
        /// 🔑 SYSTEM OWNER:
        /// TenantId == null olan kullanıcı
        /// </summary>
        public bool IsOwner =>
            IsAuthenticated && GetClaim("IsOwner") == "true";


        /// <summary>
        /// Kullanıcı login olmuş mu?
        /// </summary>
        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        // ==================================================
        // HELPERS
        // ==================================================

        private int? TryGetIntClaim(string type)
        {
            var value = GetClaim(type);
            return int.TryParse(value, out var result)
                ? result
                : null;
        }

        private string? GetClaim(string type)
        {
            return _httpContextAccessor.HttpContext?
                .User?
                .Claims
                .FirstOrDefault(x => x.Type == type)?
                .Value;
        }
    }
}
