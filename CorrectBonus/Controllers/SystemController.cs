using CorrectBonus.Services.Licensing;
using Microsoft.AspNetCore.Mvc;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers
{
    public class SystemController : Controller
    {
        private readonly ILicenseService _licenseService;

        public SystemController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        public async Task<IActionResult> LicenseLocked()
        {
            var state = await _licenseService.GetCurrentTenantLicenseStateAsync();

            if (state == LicenseState.Active)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
