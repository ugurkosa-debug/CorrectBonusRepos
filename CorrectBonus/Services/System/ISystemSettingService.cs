namespace CorrectBonus.Services.System
{
    public interface ISystemSettingService
    {
        string? Get(string key);
        T Get<T>(string key, T defaultValue = default!);
        int GetInt(string key, int defaultValue = 0);
        bool GetBool(string key, bool defaultValue = false);
        Dictionary<string, string> GetGroup(string group);

        // 🔥 THEME
        void ResetThemeToDefault();
        void Set(string key, string value);
        void SaveTheme(Dictionary<string, string> values);

    }
}
