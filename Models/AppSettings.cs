namespace KeepOn;

internal sealed class AppSettings
{
    public bool StartWithWindows { get; set; }

    public AwakeMode LastMode { get; set; } = AwakeMode.Disabled;

    public bool RestoreLastModeOnStart { get; set; }

    public bool DisableOnSessionLock { get; set; } = true;

    public bool ShowNotifications { get; set; } = true;

    public string LogLevel { get; set; } = "Info";
}
