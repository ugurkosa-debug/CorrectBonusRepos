using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CorrectBonus.Authorization;

namespace CorrectBonus.Controllers.System
{
    [Route("debug/localization")]
    public class DebugLocalizationController : Controller
    {
        private readonly IStringLocalizer _common;

        public DebugLocalizationController(IStringLocalizerFactory factory)
        {
            _common = factory.Create(
                "Shared.Common.CommonResource",
                typeof(DebugLocalizationController).Assembly.GetName().Name!
            );
        }


        [HttpGet("common")]
        public IActionResult Common()
        {
            var status = _common["Status"];
            var actions = _common["Actions"];

            return Json(new
            {
                StatusValue = status.Value,
                StatusNotFound = status.ResourceNotFound,
                ActionsValue = actions.Value,
                ActionsNotFound = actions.ResourceNotFound,
                CurrentUICulture = global::System.Globalization.CultureInfo.CurrentUICulture.Name
            });
        }
    }
}
