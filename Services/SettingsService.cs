using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeepOn;

internal sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppSettings Load()
    {
        try
        {
            MigrateLegacySettings();

            if (!File.Exists(AppPaths.SettingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(AppPaths.SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(AppPaths.SettingsPath, json);
    }

    private static void MigrateLegacySettings()
    {
        if (File.Exists(AppPaths.SettingsPath) || !File.Exists(AppPaths.LegacySettingsPath))
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        File.Copy(AppPaths.LegacySettingsPath, AppPaths.SettingsPath, overwrite: false);
    }
}
