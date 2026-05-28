namespace KeepOn;

internal static class AppPaths
{
    public const string AppName = "KeepOn";
    public const string LegacyAppName = "DominTray";

    public static string AppDataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);

    public static string LegacyAppDataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LegacyAppName);

    public static string LogsDirectory { get; } = Path.Combine(AppDataDirectory, "Logs");

    public static string SettingsPath { get; } = Path.Combine(AppDataDirectory, "settings.json");

    public static string LegacySettingsPath { get; } = Path.Combine(LegacyAppDataDirectory, "settings.json");
}
