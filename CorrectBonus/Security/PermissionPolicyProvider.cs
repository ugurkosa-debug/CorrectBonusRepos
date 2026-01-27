using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CorrectBonus.Security
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Beklenen format: PERMISSION:ROLES_VIEW
            if (policyName.StartsWith("PERMISSION:"))
            {
                var permissionCode = policyName.Substring("PERMISSION:".Length);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permissionCode))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
