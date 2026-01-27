using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CorrectBonus.Services.System
{
    public class MenuValidationService
    {
        private readonly IActionDescriptorCollectionProvider _provider;

        public MenuValidationService(
            IActionDescriptorCollectionProvider provider)
        {
            _provider = provider;
        }

        public bool IsValid(string? controller, string? action)
        {
            if (string.IsNullOrWhiteSpace(controller))
                return true; // container menu

            return _provider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Any(x =>
                    string.Equals(x.ControllerName, controller, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.ActionName, action ?? "Index", StringComparison.OrdinalIgnoreCase));
        }
    }
}
