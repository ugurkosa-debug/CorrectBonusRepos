using Microsoft.Extensions.Localization;

namespace CorrectBonus.Services.Localization
{
    public interface ICommonLocalizer
    {
        LocalizedString this[string key] { get; }
    }

    public class CommonLocalizer : ICommonLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public CommonLocalizer(IStringLocalizerFactory factory)
        {
            // ResourcesPath = "Resources"
            // => baseName = Resources klasöründen sonrası
            _localizer = factory.Create(
                "Shared.Common.CommonResource",
                typeof(CommonLocalizer).Assembly.GetName().Name!
            );
        }

        public LocalizedString this[string key] => _localizer[key];
    }
}
