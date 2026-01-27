using CorrectBonus.Entities.Authorization;

namespace CorrectBonus.Services.Auth
{
    public interface IPermissionService
    {
        /// <summary>
        /// Checks page-level permission (XXX_VIEW, XXX_EDIT, etc.)
        /// </summary>
        bool Has(string permissionCode);
    }
}
