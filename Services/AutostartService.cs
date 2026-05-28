using Microsoft.Win32;

namespace KeepOn;

internal sealed class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = AppPaths.AppName;
    private const string LegacyValueName = AppPaths.LegacyAppName;

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        if (string.Equals(value, Quote(GetExecutablePath()), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var legacyValue = key?.GetValue(LegacyValueName) as string;
        if (legacyValue is null)
        {
            return false;
        }

        SetEnabled(true);
        return true;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (key is null)
        {
            throw new InvalidOperationException("Cannot open HKCU Run key.");
        }

        if (enabled)
        {
            key.SetValue(ValueName, Quote(GetExecutablePath()), RegistryValueKind.String);
            key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
        }
    }

    private static string GetExecutablePath()
    {
        return Environment.ProcessPath ?? Application.ExecutablePath;
    }

    private static string Quote(string value)
    {
        return $"\"{value}\"";
    }
}
