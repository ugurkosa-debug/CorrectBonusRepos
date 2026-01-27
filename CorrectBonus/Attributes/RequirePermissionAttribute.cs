using Microsoft.AspNetCore.Authorization;

namespace CorrectBonus.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permissionCode)
        {
            Policy = $"PERMISSION:{permissionCode}";
        }
    }
}
