using CorrectBonus.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorrectBonus.Controllers.System
{
    public class LicenseController : Controller
    {
        public IActionResult Required()
            => View();

        public IActionResult Expired()
            => View();
    }

}
