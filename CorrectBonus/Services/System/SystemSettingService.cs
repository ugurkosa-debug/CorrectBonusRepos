using CorrectBonus.Data;
using CorrectBonus.Entities.System;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CorrectBonus.Services.System
{
    public class SystemSettingService : ISystemSettingService
    {
        private static readonly Dictionary<string, string> CssToThemeKeyMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // SIDEBAR
                ["--sidebar-bg"] = "Theme.SidebarBg",
                ["--sidebar-active"] = "Theme.SidebarActive",
                ["--menu-hover"] = "Theme.MenuHover",

                // BUTTONS
                ["--button-primary"] = "Theme.ButtonPrimary",
                ["--button-primary-hover"] = "Theme.ButtonPrimaryHover",
                ["--button-secondary"] = "Theme.ButtonSecondary",
                ["--button-radius"] = "Theme.BorderRadius",
                ["--radius"] = "Theme.BorderRadius",

                // LAYOUT
                ["--layout-padding"] = "Theme.LayoutPadding",
                ["--header-height"] = "Theme.HeaderHeight",

                // SIDEBAR SIZE / LOGO
                ["--sidebar-width"] = "Theme.SidebarWidth",
                ["--sidebar-logo-width"] = "Theme.SidebarLogoWidth",
                ["--sidebar-logo-height"] = "Theme.SidebarLogoHeight",

                // TABLE
                ["--col-name"] = "Theme.TableColName",
                ["--col-email"] = "Theme.TableColEmail",
                ["--col-status"] = "Theme.TableColStatus",
                ["--col-actions"] = "Theme.TableColActions",

                // TYPOGRAPHY
                ["--font-size-base"] = "Theme.FontSizeBase",
                ["--line-height-base"] = "Theme.LineHeightBase"
            };


        private readonly ApplicationDbContext _context;

        public SystemSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GetDescription(SystemSetting setting)
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en"
                ? setting.DescriptionEn
                : setting.DescriptionTr;
        }

        // ===============================
        // GET STRING
        // ===============================
        public string? Get(string key)
        {
            var active = _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefault(x => x.SettingKey == key && x.IsActive);

            if (active != null)
                return active.Value;

            // 🔥 fallback → default value
            return _context.SystemSettings
                .AsNoTracking()
                .Where(x => x.SettingKey == key)
                .Select(x => x.DefaultValue)
                .FirstOrDefault();
        }

        // ===============================
        // GET<T>
        // ===============================
        public T Get<T>(string key, T defaultValue = default!)
        {
            var value = Get(key);

            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        // ===============================
        // GROUP GET
        // ===============================
        public Dictionary<string, string> GetGroup(string group)
        {
            return _context.SystemSettings
                .AsNoTracking()
                .Where(x => x.Group == group && x.IsActive)
                .ToDictionary(
                    x => x.SettingKey,
                    x => x.Value ?? string.Empty
                );
        }

        // ===============================
        // HELPERS
        // ===============================
        public int GetInt(string key, int defaultValue = 0)
        {
            return int.TryParse(Get(key), out var result)
                ? result
                : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = Get(key);
            return value != null &&
                   value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        // ===============================
        // 🔥 THEME RESET (GLOBAL)
        // ===============================
        public void ResetThemeToDefault()
        {
            var themeSettings = _context.SystemSettings
                .Where(x => x.Group == "Theme" && x.DefaultValue != null)
                .ToList();

            foreach (var setting in themeSettings)
            {
                setting.Value = setting.DefaultValue!;
                setting.IsActive = true;
            }

            _context.SaveChanges();
        }
        public void Set(string key, string value)
        {
            var setting = _context.SystemSettings
                .FirstOrDefault(x => x.SettingKey == key);

            if (setting == null)
                return;

            setting.Value = value;
            setting.IsActive = true;
            setting.IsDefault = false;

            _context.SaveChanges();
        }
        public void SaveTheme(Dictionary<string, string> values)
        {
            if (values == null || values.Count == 0)
                return;

            foreach (var (cssVar, cssValue) in values)
            {
                if (!CssToThemeKeyMap.TryGetValue(cssVar, out var themeKey))
                    continue;

                var setting = _context.SystemSettings
                    .FirstOrDefault(x => x.SettingKey == themeKey);

                if (setting == null)
                    continue;

                setting.Value = cssValue;
                setting.IsActive = true;
                setting.IsDefault = false;
            }

            _context.SaveChanges();
        }
    }
}
