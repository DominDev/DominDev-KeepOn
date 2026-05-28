namespace KeepOn;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly SynchronizationContext _syncContext;
    private readonly PowerRequestService _powerRequestService;
    private readonly SettingsService _settingsService;
    private readonly LoggingService _loggingService;
    private readonly AutostartService _autostartService;
    private readonly SessionStateService _sessionStateService;
    private readonly AppSettings _settings;
    private readonly ContextMenuStrip _menu;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusMenuItem;
    private readonly ToolStripMenuItem _openPanelMenuItem;
    private readonly ToolStripMenuItem _disabledMenuItem;
    private readonly ToolStripMenuItem _systemMenuItem;
    private readonly ToolStripMenuItem _systemAndDisplayMenuItem;
    private readonly ToolStripMenuItem _startWithWindowsMenuItem;
    private readonly ToolStripMenuItem _disableOnSessionLockMenuItem;
    private readonly ToolStripMenuItem _guideMenuItem;
    private readonly ToolStripMenuItem _aboutMenuItem;

    private AwakeMode _mode = AwakeMode.Disabled;
    private ModernControlPanelForm? _panelForm;

    public TrayApplicationContext()
    {
        _syncContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _powerRequestService = new PowerRequestService();
        _settingsService = new SettingsService();
        _loggingService = new LoggingService();
        _autostartService = new AutostartService();
        _settings = _settingsService.Load();
        try
        {
            _settings.StartWithWindows = _autostartService.IsEnabled();
        }
        catch (Exception exception)
        {
            _loggingService.Error("Autostart status read failed.", exception);
            _settings.StartWithWindows = false;
        }

        _sessionStateService = new SessionStateService(() =>
            _syncContext.Post(_ => HandleSessionLock(), null));

        _statusMenuItem = new ToolStripMenuItem("Status: Disabled")
        {
            Enabled = false
        };

        _openPanelMenuItem = new ToolStripMenuItem("Open panel", null, (_, _) => ShowPanel());
        _disabledMenuItem = new ToolStripMenuItem("Disabled", null, (_, _) => SetMode(AwakeMode.Disabled));
        _systemMenuItem = new ToolStripMenuItem("Keep system awake", null, (_, _) => SetMode(AwakeMode.SystemRequired));
        _systemAndDisplayMenuItem = new ToolStripMenuItem("Keep system + display awake", null, (_, _) => SetMode(AwakeMode.SystemAndDisplayRequired));
        _startWithWindowsMenuItem = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleStartWithWindows());
        _disableOnSessionLockMenuItem = new ToolStripMenuItem("Disable on session lock", null, (_, _) => ToggleDisableOnSessionLock());
        _guideMenuItem = new ToolStripMenuItem("Guide", null, (_, _) => ShowPanel(ControlPanelView.Guide));
        _aboutMenuItem = new ToolStripMenuItem("About", null, (_, _) => ShowPanel(ControlPanelView.About));

        var exitMenuItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitThread());

        _menu = new ContextMenuStrip();
        _menu.Items.Add(_statusMenuItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_openPanelMenuItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_disabledMenuItem);
        _menu.Items.Add(_systemMenuItem);
        _menu.Items.Add(_systemAndDisplayMenuItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_startWithWindowsMenuItem);
        _menu.Items.Add(_disableOnSessionLockMenuItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_guideMenuItem);
        _menu.Items.Add(_aboutMenuItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(exitMenuItem);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _menu,
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application,
            Text = "KeepOn - Disabled",
            Visible = true
        };
        _notifyIcon.MouseUp += OnNotifyIconMouseUp;

        UpdateStatus();
        SaveSettings();
        _loggingService.Info("Application started.");
    }

    protected override void ExitThreadCore()
    {
        _loggingService.Info("Application exiting.");
        _panelForm?.Close();
        _panelForm?.Dispose();
        _sessionStateService.Dispose();
        _powerRequestService.Dispose();
        _notifyIcon.MouseUp -= OnNotifyIconMouseUp;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _loggingService.Info("Application exited.");
        base.ExitThreadCore();
    }

    private void SetMode(AwakeMode mode)
    {
        try
        {
            _powerRequestService.SetMode(mode);
            _mode = _powerRequestService.CurrentMode;
            _settings.LastMode = _mode;
            SaveSettings();
            _loggingService.Info($"Mode changed: {_mode}.");
            UpdateStatus();
        }
        catch (Exception exception) when (exception is PowerRequestException or InvalidOperationException)
        {
            _mode = _powerRequestService.CurrentMode;
            UpdateStatus();
            _loggingService.Error($"Mode change failed: {mode}.", exception);
            ShowNotification(AppPaths.AppName, exception.Message, ToolTipIcon.Error);
        }
    }

    private void UpdateStatus()
    {
        _disabledMenuItem.Checked = _mode == AwakeMode.Disabled;
        _systemMenuItem.Checked = _mode == AwakeMode.SystemRequired;
        _systemAndDisplayMenuItem.Checked = _mode == AwakeMode.SystemAndDisplayRequired;
        _startWithWindowsMenuItem.Checked = _settings.StartWithWindows;
        _disableOnSessionLockMenuItem.Checked = _settings.DisableOnSessionLock;

        var status = _mode switch
        {
            AwakeMode.SystemRequired => "System awake",
            AwakeMode.SystemAndDisplayRequired => "System and display awake",
            _ => "Disabled"
        };

        _statusMenuItem.Text = $"Status: {status}";
        _notifyIcon.Text = $"{AppPaths.AppName} - {status}";
        _panelForm?.SetState(_mode, _settings.StartWithWindows, _settings.DisableOnSessionLock);
    }

    private void ShowPanel(ControlPanelView view = ControlPanelView.Dashboard)
    {
        if (_panelForm is null || _panelForm.IsDisposed)
        {
            _panelForm = new ModernControlPanelForm(
                SetMode,
                ToggleStartWithWindows,
                ToggleDisableOnSessionLock,
                ExitThread);
            _panelForm.FormClosed += (_, _) => _panelForm = null;
        }

        _panelForm.SetState(_mode, _settings.StartWithWindows, _settings.DisableOnSessionLock);
        _panelForm.ShowView(view);

        if (_panelForm.WindowState == FormWindowState.Minimized)
        {
            _panelForm.WindowState = FormWindowState.Normal;
        }

        _panelForm.Show();
        _panelForm.Activate();
    }

    private void OnNotifyIconMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowPanel();
        }
    }

    private void ToggleStartWithWindows()
    {
        var enabled = !_settings.StartWithWindows;

        try
        {
            _autostartService.SetEnabled(enabled);
            _settings.StartWithWindows = enabled;
            SaveSettings();
            _loggingService.Info($"Start with Windows changed: {enabled}.");
            UpdateStatus();
        }
        catch (Exception exception)
        {
            _loggingService.Error("Autostart change failed.", exception);
            ShowNotification(AppPaths.AppName, "Could not update Windows startup setting.", ToolTipIcon.Error);
        }
    }

    private void ToggleDisableOnSessionLock()
    {
        _settings.DisableOnSessionLock = !_settings.DisableOnSessionLock;
        SaveSettings();
        _loggingService.Info($"Disable on session lock changed: {_settings.DisableOnSessionLock}.");
        UpdateStatus();
    }

    private void HandleSessionLock()
    {
        if (!_settings.DisableOnSessionLock || _mode == AwakeMode.Disabled)
        {
            return;
        }

        try
        {
            _loggingService.Info("SessionLock detected. Disabling active power requests.");
            _powerRequestService.Disable();
            _mode = AwakeMode.Disabled;
            _settings.LastMode = AwakeMode.Disabled;
            SaveSettings();
            UpdateStatus();
            ShowNotification(AppPaths.AppName, "Power requests disabled after session lock.", ToolTipIcon.Info);
        }
        catch (Exception exception) when (exception is PowerRequestException or InvalidOperationException)
        {
            _mode = _powerRequestService.CurrentMode;
            UpdateStatus();
            _loggingService.Error("SessionLock disable failed.", exception);
            ShowNotification(AppPaths.AppName, exception.Message, ToolTipIcon.Error);
        }
    }

    private void SaveSettings()
    {
        try
        {
            _settingsService.Save(_settings);
        }
        catch (Exception exception)
        {
            _loggingService.Error("Settings save failed.", exception);
            ShowNotification(AppPaths.AppName, "Could not save settings.", ToolTipIcon.Warning);
        }
    }

    private void ShowNotification(string title, string text, ToolTipIcon icon)
    {
        if (!_settings.ShowNotifications)
        {
            return;
        }

        _notifyIcon.ShowBalloonTip(5000, title, text, icon);
    }

}
