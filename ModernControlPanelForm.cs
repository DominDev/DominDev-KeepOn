using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace KeepOn;

internal sealed class ModernControlPanelForm : Form
{
    private static readonly Color WindowBackground = Color.FromArgb(246, 248, 251);
    private static readonly Color SidebarBackground = Color.FromArgb(248, 250, 252);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
    private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
    private static readonly Color Border = Color.FromArgb(226, 232, 240);
    private static readonly Color Accent = Color.FromArgb(37, 99, 235);
    private static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    private static readonly Color Success = Color.FromArgb(34, 197, 94);
    private static readonly Color Danger = Color.FromArgb(220, 38, 38);
    private static readonly Color DisabledGray = Color.FromArgb(148, 163, 184);

    private readonly Action<AwakeMode> _setMode;
    private readonly Action _toggleStartWithWindows;
    private readonly Action _toggleDisableOnSessionLock;
    private readonly Action _exitApplication;

    private readonly Label _statusTitleLabel;
    private readonly Label _statusDescriptionLabel;
    private readonly StatusSummaryIcon _statusSummaryIcon;
    private readonly ModeTile _disabledTile;
    private readonly ModeTile _systemTile;
    private readonly ModeTile _systemAndDisplayTile;
    private readonly ToggleRow _startWithWindowsToggle;
    private readonly ToggleRow _disableOnSessionLockToggle;
    private readonly FooterStatus _footerStatus;
    private readonly Control[] _dashboardControls;
    private readonly Panel _aboutView;
    private readonly Panel _guideView;
    private SidebarButton _dashboardNavButton = null!;
    private SidebarButton _aboutNavButton = null!;
    private SidebarButton _guideNavButton = null!;
    private bool _updating;

    public ModernControlPanelForm(
        Action<AwakeMode> setMode,
        Action toggleStartWithWindows,
        Action toggleDisableOnSessionLock,
        Action exitApplication)
    {
        _setMode = setMode;
        _toggleStartWithWindows = toggleStartWithWindows;
        _toggleDisableOnSessionLock = toggleDisableOnSessionLock;
        _exitApplication = exitApplication;

        Text = AppPaths.AppName;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(1156, 844);
        MinimumSize = new Size(1156, 844);
        BackColor = WindowBackground;
        Font = new Font("Segoe UI", 10F);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var shell = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = WindowBackground
        };

        var sidebar = CreateSidebar();
        shell.Controls.Add(sidebar);

        var content = new Panel
        {
            Location = new Point(260, 0),
            Size = new Size(896, 844),
            BackColor = WindowBackground
        };

        var titleLabel = new Label
        {
            AutoSize = false,
            Text = "Dashboard",
            Font = new Font("Segoe UI Semibold", 27F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Location = new Point(48, 72),
            Size = new Size(420, 54)
        };

        var subtitleLabel = new Label
        {
            AutoSize = false,
            Text = "Session-aware power control",
            Font = new Font("Segoe UI", 15F),
            ForeColor = TextSecondary,
            Location = new Point(50, 124),
            Size = new Size(460, 34)
        };

        var statusCard = CreateCard(new Point(48, 184), new Size(800, 160));
        statusCard.Controls.Add(CreateCaption("Current status", 28, 24, 220));
        _statusSummaryIcon = new StatusSummaryIcon
        {
            Location = new Point(28, 74),
            Size = new Size(52, 52),
            StatusColor = Success,
            Mode = AwakeMode.Disabled,
            BackColor = CardBackground
        };
        _statusTitleLabel = new Label
        {
            AutoSize = false,
            Text = "Disabled",
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = CardBackground,
            Location = new Point(104, 71),
            Size = new Size(500, 38)
        };
        _statusDescriptionLabel = new Label
        {
            AutoSize = false,
            Text = "Power requests are disabled.",
            Font = new Font("Segoe UI", 11.2F),
            ForeColor = TextSecondary,
            BackColor = CardBackground,
            Location = new Point(106, 111),
            Size = new Size(520, 28)
        };
        var illustration = new DisplayIllustration
        {
            Location = new Point(672, 32),
            Size = new Size(96, 96),
            BackColor = CardBackground
        };
        statusCard.Controls.Add(_statusSummaryIcon);
        statusCard.Controls.Add(_statusTitleLabel);
        statusCard.Controls.Add(_statusDescriptionLabel);
        statusCard.Controls.Add(illustration);

        var modeCard = CreateCard(new Point(48, 368), new Size(800, 184));
        modeCard.Controls.Add(CreateCaption("Mode", 28, 24, 120));
        _disabledTile = CreateModeTile("Disabled", "Normal Windows behavior", ModeIcon.Disabled, new Point(28, 66));
        _disabledTile.Click += (_, _) => _setMode(AwakeMode.Disabled);
        _systemTile = CreateModeTile("System", "Prevent sleep", ModeIcon.System, new Point(286, 66));
        _systemTile.Click += (_, _) => _setMode(AwakeMode.SystemRequired);
        _systemAndDisplayTile = CreateModeTile("System + display", "Prevent sleep and screen off", ModeIcon.SystemAndDisplay, new Point(544, 66));
        _systemAndDisplayTile.Click += (_, _) => _setMode(AwakeMode.SystemAndDisplayRequired);
        modeCard.Controls.Add(_disabledTile);
        modeCard.Controls.Add(_systemTile);
        modeCard.Controls.Add(_systemAndDisplayTile);

        var settingsCard = CreateCard(new Point(48, 576), new Size(800, 208));
        settingsCard.Controls.Add(CreateCaption("Settings", 28, 22, 160));
        _startWithWindowsToggle = CreateToggleRow(
            "Start with Windows",
            "Launch KeepOn automatically after sign-in.",
            28,
            56);
        _startWithWindowsToggle.CheckedChanged += (_, _) =>
        {
            if (!_updating)
            {
                _toggleStartWithWindows();
            }
        };
        _disableOnSessionLockToggle = CreateToggleRow(
            "Disable on session lock",
            "Automatically disable power control when the session is locked.",
            28,
            116);
        _disableOnSessionLockToggle.CheckedChanged += (_, _) =>
        {
            if (!_updating)
            {
                _toggleDisableOnSessionLock();
            }
        };
        settingsCard.Controls.Add(_startWithWindowsToggle);
        settingsCard.Controls.Add(_disableOnSessionLockToggle);

        var versionLabel = new Label
        {
            AutoSize = false,
            Text = $"{AppPaths.AppName}  v{Application.ProductVersion}",
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextSecondary,
            BackColor = WindowBackground,
            Location = new Point(48, 808),
            Size = new Size(220, 24)
        };

        _footerStatus = new FooterStatus
        {
            StatusText = "Running",
            DotColor = Success,
            Location = new Point(700, 806),
            Size = new Size(148, 28),
            BackColor = WindowBackground
        };

        content.Controls.Add(titleLabel);
        content.Controls.Add(subtitleLabel);
        content.Controls.Add(statusCard);
        content.Controls.Add(modeCard);
        content.Controls.Add(settingsCard);
        content.Controls.Add(versionLabel);
        content.Controls.Add(_footerStatus);

        _dashboardControls =
        [
            titleLabel,
            subtitleLabel,
            statusCard,
            modeCard,
            settingsCard,
            versionLabel,
            _footerStatus
        ];

        _aboutView = CreateAboutView();
        _guideView = CreateGuideView();
        _aboutView.Visible = false;
        _guideView.Visible = false;
        content.Controls.Add(_aboutView);
        content.Controls.Add(_guideView);
        shell.Controls.Add(content);

        Controls.Add(shell);
        ShowView(ControlPanelView.Dashboard);
    }

    public void SetState(AwakeMode mode, bool startWithWindows, bool disableOnSessionLock)
    {
        _updating = true;
        try
        {
            _startWithWindowsToggle.Checked = startWithWindows;
            _disableOnSessionLockToggle.Checked = disableOnSessionLock;
        }
        finally
        {
            _updating = false;
        }

        _statusTitleLabel.Text = mode switch
        {
            AwakeMode.SystemRequired => "System awake",
            AwakeMode.SystemAndDisplayRequired => "System and display awake",
            _ => "Disabled"
        };

        _statusDescriptionLabel.Text = mode switch
        {
            AwakeMode.SystemRequired => "Your system is active. The display can still turn off.",
            AwakeMode.SystemAndDisplayRequired => "Your system and display are active.",
            _ => "Windows can sleep or turn off the display normally."
        };

        _statusSummaryIcon.Mode = mode;
        _statusSummaryIcon.StatusColor = mode switch
        {
            AwakeMode.SystemRequired => Accent,
            AwakeMode.SystemAndDisplayRequired => Success,
            _ => DisabledGray
        };

        _disabledTile.Active = mode == AwakeMode.Disabled;
        _systemTile.Active = mode == AwakeMode.SystemRequired;
        _systemAndDisplayTile.Active = mode == AwakeMode.SystemAndDisplayRequired;
        _footerStatus.DotColor = mode == AwakeMode.Disabled ? DisabledGray : Success;
        _footerStatus.StatusText = mode == AwakeMode.Disabled ? "Idle" : "Running";
    }

    private Panel CreateSidebar()
    {
        var sidebar = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(260, 844),
            BackColor = SidebarBackground
        };
        sidebar.Paint += (_, e) =>
        {
            using var pen = new Pen(Border);
            e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
        };

        var icon = new AppLogo
        {
            Location = new Point(28, 34),
            Size = new Size(40, 40),
            AccentColor = Accent,
            BackColor = SidebarBackground
        };
        var title = new Label
        {
            AutoSize = false,
            Text = AppPaths.AppName,
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = SidebarBackground,
            Location = new Point(82, 38),
            Size = new Size(150, 36)
        };

        _dashboardNavButton = CreateSidebarButton("Dashboard", SideIcon.Home, true, 24, 122);
        _dashboardNavButton.Click += (_, _) => ShowView(ControlPanelView.Dashboard);
        _aboutNavButton = CreateSidebarButton("About", SideIcon.Info, false, 24, 184);
        _aboutNavButton.Click += (_, _) => ShowView(ControlPanelView.About);

        var dividerTop = new Panel { BackColor = Border, Location = new Point(28, 634), Size = new Size(204, 1) };
        _guideNavButton = CreateSidebarButton("Guide", SideIcon.Book, false, 24, 658);
        _guideNavButton.Click += (_, _) => ShowView(ControlPanelView.Guide);
        var website = CreateSidebarButton("Website", SideIcon.Globe, false, 24, 714);
        website.Click += (_, _) => OpenWebsite();
        var dividerBottom = new Panel { BackColor = Border, Location = new Point(28, 768), Size = new Size(204, 1) };
        var exit = CreateSidebarButton("Exit", SideIcon.Exit, false, 24, 782);
        exit.ForeColor = Danger;
        exit.IconColor = Danger;
        exit.Click += (_, _) => _exitApplication();

        sidebar.Controls.Add(icon);
        sidebar.Controls.Add(title);
        sidebar.Controls.Add(_dashboardNavButton);
        sidebar.Controls.Add(_aboutNavButton);
        sidebar.Controls.Add(dividerTop);
        sidebar.Controls.Add(_guideNavButton);
        sidebar.Controls.Add(website);
        sidebar.Controls.Add(dividerBottom);
        sidebar.Controls.Add(exit);
        return sidebar;
    }

    internal void ShowView(ControlPanelView view)
    {
        var dashboardVisible = view == ControlPanelView.Dashboard;
        foreach (var control in _dashboardControls)
        {
            control.Visible = dashboardVisible;
        }

        _aboutView.Visible = view == ControlPanelView.About;
        _guideView.Visible = view == ControlPanelView.Guide;
        if (_aboutView.Visible)
        {
            _aboutView.BringToFront();
        }

        if (_guideView.Visible)
        {
            _guideView.BringToFront();
        }

        SetSidebarButtonState(_dashboardNavButton, view == ControlPanelView.Dashboard);
        SetSidebarButtonState(_aboutNavButton, view == ControlPanelView.About);
        SetSidebarButtonState(_guideNavButton, view == ControlPanelView.Guide);
    }

    private static void SetSidebarButtonState(SidebarButton button, bool active)
    {
        button.Active = active;
        button.Font = new Font("Segoe UI Semibold", active ? 10.5F : 10.2F, active ? FontStyle.Bold : FontStyle.Regular);
        button.ForeColor = active ? Accent : TextPrimary;
        button.IconColor = active ? Accent : Color.FromArgb(58, 68, 83);
        button.Invalidate();
    }

    private static Panel CreateAboutView()
    {
        var view = CreateView();
        view.Controls.Add(CreateViewTitle("About", "Application details and support information"));

        var infoCard = CreateCard(new Point(48, 184), new Size(800, 274));
        infoCard.Controls.Add(CreateCaption("About KeepOn", 28, 24, 220));
        infoCard.Controls.Add(CreateBodyLabel(
            "KeepOn keeps Windows awake only when you explicitly enable one of the power-control modes. It runs from the notification area and clears active requests when the app exits.",
            28,
            68,
            720,
            58));
        infoCard.Controls.Add(CreateInfoRow("Version", Application.ProductVersion, 28, 144));
        infoCard.Controls.Add(CreateInfoRow("Website", "https://domindev.com", 28, 184));
        infoCard.Controls.Add(CreateInfoRow("Runtime", ".NET 10 / Windows Forms", 28, 224));

        var footer = CreateFooterVersionLabel();
        view.Controls.Add(infoCard);
        view.Controls.Add(footer);
        return view;
    }

    private static Panel CreateGuideView()
    {
        var view = CreateView();
        view.Controls.Add(CreateViewTitle("Guide", "How the tray controls behave"));

        var modesCard = CreateCard(new Point(48, 184), new Size(800, 250));
        modesCard.Controls.Add(CreateCaption("Modes", 28, 24, 180));
        modesCard.Controls.Add(CreateGuideRow("Disabled", "Normal Windows behavior. The system may sleep and the display may turn off.", 28, 66));
        modesCard.Controls.Add(CreateGuideRow("System", "Prevents system sleep while still allowing the display to turn off.", 28, 122));
        modesCard.Controls.Add(CreateGuideRow("System + display", "Prevents system sleep and keeps the display awake.", 28, 178));

        var settingsCard = CreateCard(new Point(48, 458), new Size(800, 250));
        settingsCard.Controls.Add(CreateCaption("Settings and actions", 28, 24, 240));
        settingsCard.Controls.Add(CreateGuideRow("Start with Windows", "Adds or removes KeepOn from the current user's Windows startup entry.", 28, 66));
        settingsCard.Controls.Add(CreateGuideRow("Disable on session lock", "Disables active power requests when the Windows session is locked.", 28, 122));
        settingsCard.Controls.Add(CreateGuideRow("Exit", "Closes the tray app and clears active power requests.", 28, 178));

        view.Controls.Add(modesCard);
        view.Controls.Add(settingsCard);
        view.Controls.Add(CreateFooterVersionLabel());
        return view;
    }

    private static Panel CreateView()
    {
        return new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(896, 844),
            BackColor = WindowBackground
        };
    }

    private static Control CreateViewTitle(string title, string subtitle)
    {
        var container = new Panel
        {
            Location = new Point(48, 72),
            Size = new Size(640, 90),
            BackColor = WindowBackground
        };
        container.Controls.Add(new Label
        {
            AutoSize = false,
            Text = title,
            Font = new Font("Segoe UI Semibold", 27F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = WindowBackground,
            Location = new Point(0, 0),
            Size = new Size(520, 54)
        });
        container.Controls.Add(new Label
        {
            AutoSize = false,
            Text = subtitle,
            Font = new Font("Segoe UI", 15F),
            ForeColor = TextSecondary,
            BackColor = WindowBackground,
            Location = new Point(2, 52),
            Size = new Size(620, 34)
        });
        return container;
    }

    private static Label CreateBodyLabel(string text, int x, int y, int width, int height)
    {
        return new Label
        {
            AutoSize = false,
            Text = text,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = TextSecondary,
            BackColor = CardBackground,
            Location = new Point(x, y),
            Size = new Size(width, height)
        };
    }

    private static Control CreateInfoRow(string title, string value, int x, int y)
    {
        var row = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(720, 30),
            BackColor = CardBackground
        };
        row.Controls.Add(new Label
        {
            AutoSize = false,
            Text = title,
            Font = new Font("Segoe UI Semibold", 9.8F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = CardBackground,
            Location = new Point(0, 2),
            Size = new Size(130, 24)
        });
        row.Controls.Add(new Label
        {
            AutoSize = false,
            Text = value,
            Font = new Font("Segoe UI", 9.8F),
            ForeColor = TextSecondary,
            BackColor = CardBackground,
            Location = new Point(140, 2),
            Size = new Size(560, 24)
        });
        return row;
    }

    private static Control CreateGuideRow(string title, string description, int x, int y)
    {
        var row = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(744, 48),
            BackColor = CardBackground
        };
        row.Controls.Add(new Label
        {
            AutoSize = false,
            Text = title,
            Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = CardBackground,
            Location = new Point(0, 0),
            Size = new Size(180, 24)
        });
        row.Controls.Add(new Label
        {
            AutoSize = false,
            Text = description,
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = TextSecondary,
            BackColor = CardBackground,
            Location = new Point(0, 23),
            Size = new Size(720, 22)
        });
        return row;
    }

    private static Label CreateFooterVersionLabel()
    {
        return new Label
        {
            AutoSize = false,
            Text = $"{AppPaths.AppName}  v{Application.ProductVersion}",
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextSecondary,
            BackColor = WindowBackground,
            Location = new Point(48, 808),
            Size = new Size(220, 24)
        };
    }

    private static RoundedPanel CreateCard(Point location, Size size)
    {
        return new RoundedPanel
        {
            Location = location,
            Size = size,
            FillColor = CardBackground,
            BorderColor = Border,
            Radius = 16,
            ShadowColor = Color.FromArgb(12, 15, 23, 42)
        };
    }

    private static Label CreateCaption(string text, int x, int y, int width)
    {
        return new Label
        {
            AutoSize = false,
            Text = text,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = CardBackground,
            Location = new Point(x, y),
            Size = new Size(width, 28)
        };
    }

    private static ModeTile CreateModeTile(string text, string description, ModeIcon icon, Point location)
    {
        return new ModeTile
        {
            Text = text,
            Description = description,
            Icon = icon,
            Location = location,
            Size = new Size(228, 94),
            Cursor = Cursors.Hand,
            BackColor = CardBackground,
            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            AccentColor = Accent,
            AccentHoverColor = AccentHover,
            BorderColor = Border
        };
    }

    private static ToggleRow CreateToggleRow(string title, string description, int x, int y)
    {
        return new ToggleRow
        {
            Title = title,
            Description = description,
            Location = new Point(x, y),
            Size = new Size(744, 52),
            BackColor = CardBackground,
            AccentColor = Accent,
            TrackOffColor = Color.FromArgb(203, 213, 225),
            ForeColor = TextPrimary,
            TitleFont = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            DescriptionFont = new Font("Segoe UI", 8.8F)
        };
    }

    private static SidebarButton CreateSidebarButton(string text, SideIcon icon, bool active, int x, int y)
    {
        return new SidebarButton
        {
            Text = text,
            Icon = icon,
            Active = active,
            Location = new Point(x, y),
            Size = new Size(212, 52),
            Font = new Font("Segoe UI Semibold", active ? 10.5F : 10.2F, active ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = active ? Accent : TextPrimary,
            IconColor = active ? Accent : Color.FromArgb(58, 68, 83),
            Cursor = Cursors.Hand
        };
    }

    private static void OpenWebsite()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://domindev.com",
            UseShellExecute = true
        });
    }
}

internal enum ControlPanelView
{
    Dashboard,
    About,
    Guide
}

internal enum SideIcon
{
    Home,
    Info,
    Book,
    Globe,
    Exit
}

internal enum ModeIcon
{
    Disabled,
    System,
    SystemAndDisplay
}

internal sealed class RoundedPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = 10;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = Color.White;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(220, 226, 234);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ShadowColor { get; set; } = Color.Transparent;

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (ShadowColor.A > 0)
        {
            using var shadowPath = DrawingHelpers.CreateRoundRect(new Rectangle(2, 5, Width - 5, Height - 8), Radius);
            using var shadowBrush = new SolidBrush(ShadowColor);
            e.Graphics.FillPath(shadowBrush, shadowPath);
        }

        var rect = new Rectangle(0, 0, Width - 4, Height - 6);
        using var path = DrawingHelpers.CreateRoundRect(rect, Radius);
        using var brush = new SolidBrush(FillColor);
        using var pen = new Pen(BorderColor);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class SidebarButton : Control
{
    private bool _hovered;
    private bool _active;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SideIcon Icon { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color IconColor { get; set; }

    public SidebarButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (Active || _hovered)
        {
            var fill = Active ? Color.FromArgb(234, 242, 255) : Color.FromArgb(241, 245, 249);
            using var path = DrawingHelpers.CreateRoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 12);
            using var brush = new SolidBrush(fill);
            e.Graphics.FillPath(brush, path);
        }

        if (Active)
        {
            using var indicatorBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
            using var indicatorPath = DrawingHelpers.CreateRoundRect(new Rectangle(0, 13, 3, Height - 26), 2);
            e.Graphics.FillPath(indicatorBrush, indicatorPath);
        }

        DrawingHelpers.DrawSideIcon(e.Graphics, Icon, IconColor, new Rectangle(22, 17, 22, 22));
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            new Rectangle(58, 0, Width - 62, Height),
            ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}

internal sealed class AppLogo : Control
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; } = Color.FromArgb(37, 99, 235);

    public AppLogo()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var outer = DrawingHelpers.CreateRoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 11);
        using var outerBrush = new SolidBrush(Color.FromArgb(30, 64, 175));
        e.Graphics.FillPath(outerBrush, outer);

        using var inner = DrawingHelpers.CreateRoundRect(new Rectangle(6, 7, Width - 13, Height - 14), 6);
        using var innerBrush = new SolidBrush(Color.FromArgb(219, 234, 254));
        using var innerPen = new Pen(Color.FromArgb(96, 165, 250), 1.6F);
        e.Graphics.FillPath(innerBrush, inner);
        e.Graphics.DrawPath(innerPen, inner);

        using var screenPen = new Pen(AccentColor, 2F) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        e.Graphics.DrawLine(screenPen, 14, 25, 26, 25);
        e.Graphics.DrawLine(screenPen, 20, 25, 20, 30);
        e.Graphics.DrawLine(screenPen, 15, 31, 25, 31);

        using var dot = new SolidBrush(Color.FromArgb(34, 197, 94));
        e.Graphics.FillEllipse(dot, Width - 14, 8, 6, 6);
    }
}

internal sealed class FooterStatus : Control
{
    private Color _dotColor = Color.FromArgb(148, 163, 184);
    private string _statusText = "Idle";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color DotColor
    {
        get => _dotColor;
        set
        {
            _dotColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            Invalidate();
        }
    }

    public FooterStatus()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        ForeColor = Color.FromArgb(100, 116, 139);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var textSize = TextRenderer.MeasureText(StatusText, Font);
        var x = Width - textSize.Width - 18;
        using var dotBrush = new SolidBrush(DotColor);
        e.Graphics.FillEllipse(dotBrush, new Rectangle(x, (Height - 8) / 2, 8, 8));
        TextRenderer.DrawText(
            e.Graphics,
            StatusText,
            Font,
            new Rectangle(x + 16, 0, textSize.Width + 2, Height),
            ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}

internal sealed class StatusSummaryIcon : Control
{
    private Color _statusColor = Color.FromArgb(136, 146, 160);
    private AwakeMode _mode;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color StatusColor
    {
        get => _statusColor;
        set
        {
            _statusColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AwakeMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            Invalidate();
        }
    }

    public StatusSummaryIcon()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var background = Mode == AwakeMode.Disabled
            ? Color.FromArgb(241, 245, 249)
            : Mode == AwakeMode.SystemRequired
                ? Color.FromArgb(219, 234, 254)
                : Color.FromArgb(220, 252, 231);
        using var containerPath = DrawingHelpers.CreateRoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 14);
        using var containerBrush = new SolidBrush(background);
        e.Graphics.FillPath(containerBrush, containerPath);

        using var pen = new Pen(StatusColor, 4F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        if (Mode == AwakeMode.Disabled)
        {
            e.Graphics.DrawLine(pen, 17, 17, 35, 35);
            e.Graphics.DrawLine(pen, 35, 17, 17, 35);
        }
        else
        {
            e.Graphics.DrawLines(pen, new[] { new Point(16, 27), new Point(23, 34), new Point(37, 18) });
        }
    }
}

internal sealed class DisplayIllustration : Control
{
    public DisplayIllustration()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var containerPath = DrawingHelpers.CreateRoundRect(new Rectangle(2, 2, Width - 5, Height - 5), 20);
        using var containerBrush = new SolidBrush(Color.FromArgb(248, 250, 252));
        e.Graphics.FillPath(containerBrush, containerPath);
        using var paleBrush = new SolidBrush(Color.FromArgb(239, 246, 255));
        e.Graphics.FillEllipse(paleBrush, new Rectangle(14, 11, 68, 68));

        using var monitorBrush = new SolidBrush(Color.FromArgb(181, 211, 255));
        using var monitorPen = new Pen(Color.FromArgb(117, 171, 247), 2F);
        var screen = new Rectangle(24, 29, 50, 36);
        using var screenPath = DrawingHelpers.CreateRoundRect(screen, 7);
        e.Graphics.FillPath(monitorBrush, screenPath);
        e.Graphics.DrawPath(monitorPen, screenPath);

        using var standPen = new Pen(Color.FromArgb(39, 119, 221), 3F);
        e.Graphics.DrawLine(standPen, 49, 66, 49, 76);
        e.Graphics.DrawLine(standPen, 39, 78, 59, 78);

        using var borderBrush = new SolidBrush(Color.White);
        e.Graphics.FillEllipse(borderBrush, new Rectangle(66, 58, 32, 32));
        using var okBrush = new SolidBrush(Color.FromArgb(34, 197, 94));
        e.Graphics.FillEllipse(okBrush, new Rectangle(69, 61, 26, 26));
        using var checkPen = new Pen(Color.White, 2.4F) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        e.Graphics.DrawLines(checkPen, new[] { new Point(77, 75), new Point(82, 80), new Point(89, 70) });
    }
}

internal sealed class ModeTile : Control
{
    private bool _active;
    private bool _hovered;
    private bool _pressed;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ModeIcon Icon { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Description { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentHoverColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; }

    public ModeTile()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _pressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var border = Active ? AccentColor : _hovered ? Color.FromArgb(203, 213, 225) : BorderColor;
        var fill = Active ? Color.FromArgb(239, 246, 255) : _pressed ? Color.FromArgb(219, 234, 254) : _hovered ? Color.FromArgb(248, 250, 252) : Color.White;
        using var path = DrawingHelpers.CreateRoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 12);
        using var brush = new SolidBrush(fill);
        using var pen = new Pen(border, Active ? 2.0F : 1.0F);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);

        var iconColor = Active ? AccentColor : Color.FromArgb(100, 116, 139);
        DrawingHelpers.DrawModeIcon(e.Graphics, Icon, iconColor, new Rectangle((Width - 28) / 2, 13, 28, 28));
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            new Rectangle(8, 42, Width - 16, 24),
            Active ? AccentColor : ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        using var descriptionFont = new Font("Segoe UI", 8.2F);
        TextRenderer.DrawText(
            e.Graphics,
            Description,
            descriptionFont,
            new Rectangle(12, 65, Width - 24, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class ToggleRow : Control
{
    private bool _checked;
    private bool _hovered;
    private bool _pressed;

    public event EventHandler? CheckedChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value)
            {
                return;
            }

            _checked = value;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TrackOffColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Description { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Font TitleFont { get; set; } = new("Segoe UI Semibold", 9.8F, FontStyle.Bold);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Font DescriptionFont { get; set; } = new("Segoe UI", 8.6F);

    public ToggleRow()
    {
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _pressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var rowPath = DrawingHelpers.CreateRoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 12);
        using var rowBrush = new SolidBrush(_pressed ? Color.FromArgb(226, 232, 240) : _hovered ? Color.FromArgb(248, 250, 252) : Color.White);
        e.Graphics.FillPath(rowBrush, rowPath);

        TextRenderer.DrawText(
            e.Graphics,
            Title,
            TitleFont,
            new Rectangle(16, 7, Width - 108, 21),
            ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        TextRenderer.DrawText(
            e.Graphics,
            Description,
            DescriptionFont,
            new Rectangle(16, 28, Width - 108, 19),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

        var track = new Rectangle(Width - 68, 14, 44, 24);
        using var trackPath = DrawingHelpers.CreateRoundRect(track, 12);
        using var trackBrush = new SolidBrush(Checked ? AccentColor : TrackOffColor);
        e.Graphics.FillPath(trackBrush, trackPath);

        var thumbX = Checked ? track.Right - 21 : track.X + 3;
        using var thumbBrush = new SolidBrush(Color.White);
        e.Graphics.FillEllipse(thumbBrush, new Rectangle(thumbX, track.Y + 3, 18, 18));
    }
}

internal static class DrawingHelpers
{
    public static GraphicsPath CreateRoundRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawSideIcon(Graphics graphics, SideIcon icon, Color color, Rectangle rect)
    {
        using var pen = CreatePen(color, 1.8F);
        switch (icon)
        {
            case SideIcon.Home:
                graphics.DrawLines(pen, new[] { new Point(rect.X + 2, rect.Y + 11), new Point(rect.X + 11, rect.Y + 3), new Point(rect.X + 20, rect.Y + 11) });
                graphics.DrawRectangle(pen, rect.X + 5, rect.Y + 10, 12, 10);
                break;
            case SideIcon.Info:
                graphics.DrawEllipse(pen, rect.X + 2, rect.Y + 2, 18, 18);
                graphics.DrawLine(pen, rect.X + 11, rect.Y + 10, rect.X + 11, rect.Y + 16);
                graphics.FillEllipse(new SolidBrush(color), rect.X + 10, rect.Y + 6, 2, 2);
                break;
            case SideIcon.Book:
                graphics.DrawRectangle(pen, rect.X + 2, rect.Y + 4, 8, 15);
                graphics.DrawRectangle(pen, rect.X + 12, rect.Y + 4, 8, 15);
                break;
            case SideIcon.Globe:
                graphics.DrawEllipse(pen, rect.X + 2, rect.Y + 2, 18, 18);
                graphics.DrawLine(pen, rect.X + 3, rect.Y + 11, rect.X + 19, rect.Y + 11);
                graphics.DrawArc(pen, rect.X + 6, rect.Y + 2, 10, 18, 90, 180);
                graphics.DrawArc(pen, rect.X + 6, rect.Y + 2, 10, 18, 270, 180);
                break;
            case SideIcon.Exit:
                graphics.DrawRectangle(pen, rect.X + 2, rect.Y + 5, 10, 12);
                graphics.DrawLine(pen, rect.X + 10, rect.Y + 11, rect.X + 20, rect.Y + 11);
                graphics.DrawLine(pen, rect.X + 16, rect.Y + 7, rect.X + 20, rect.Y + 11);
                graphics.DrawLine(pen, rect.X + 16, rect.Y + 15, rect.X + 20, rect.Y + 11);
                break;
        }
    }

    public static void DrawModeIcon(Graphics graphics, ModeIcon icon, Color color, Rectangle rect)
    {
        using var pen = CreatePen(color, 2F);
        switch (icon)
        {
            case ModeIcon.Disabled:
                graphics.DrawEllipse(pen, rect.X + 3, rect.Y + 3, 22, 22);
                graphics.DrawLine(pen, rect.X + 8, rect.Y + 8, rect.X + 23, rect.Y + 23);
                break;
            case ModeIcon.System:
                graphics.DrawRectangle(pen, rect.X + 4, rect.Y + 5, 20, 15);
                graphics.DrawLine(pen, rect.X + 14, rect.Y + 20, rect.X + 14, rect.Y + 25);
                graphics.DrawLine(pen, rect.X + 9, rect.Y + 25, rect.X + 19, rect.Y + 25);
                break;
            case ModeIcon.SystemAndDisplay:
                graphics.DrawRectangle(pen, rect.X + 1, rect.Y + 5, 18, 14);
                graphics.DrawRectangle(pen, rect.X + 15, rect.Y + 13, 11, 10);
                break;
        }
    }

    private static Pen CreatePen(Color color, float width)
    {
        return new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
    }
}
