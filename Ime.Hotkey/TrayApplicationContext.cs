namespace Ime.Hotkey;

/// <summary>
/// Application context that manages tray icon and hotkey window lifecycle.
/// </summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private HotKeyWindow? _hotKeyWindow;
    private bool _isExiting;
    private Icon? _customIcon;

    public TrayApplicationContext()
    {
        _trayIcon = BuildTrayIcon();
        RegisterHotKeys();
    }

    private NotifyIcon BuildTrayIcon()
    {
        _customIcon = CreateTrayIcon();

        var trayIcon = new NotifyIcon
        {
            Icon = _customIcon ?? SystemIcons.Application,
            Visible = true,
            Text = "IME Hotkey (Ctrl+Win+Z/A)"
        };

        var registerItem = new ToolStripMenuItem("ホットキーを再登録");
        registerItem.Click += (_, _) => ReregisterHotKeys();

        var startupItem = new ToolStripMenuItem("Windows起動時に実行")
        {
            Checked = StartupService.IsEnabled(),
            CheckOnClick = true
        };
        startupItem.CheckedChanged += (_, _) => 
            StartupService.SetEnabled(startupItem.Checked);

        var exitItem = new ToolStripMenuItem("終了");
        exitItem.Click += (_, _) => ExitApplication();

        trayIcon.ContextMenuStrip = new ContextMenuStrip();
        trayIcon.ContextMenuStrip.Items.Add(registerItem);
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        trayIcon.ContextMenuStrip.Items.Add(startupItem);
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        trayIcon.ContextMenuStrip.Items.Add(exitItem);

        trayIcon.DoubleClick += (_, _) => 
            trayIcon.ShowBalloonTip(2000, "IME Hotkey", 
                "Ctrl+Win+Z: 日本語\nCtrl+Win+A: 英語", ToolTipIcon.Info);

        return trayIcon;
    }

    /// <summary>
    /// Create a custom tray icon with "あ" character (Japanese Hiragana "a")
    /// </summary>
    private static Icon CreateTrayIcon()
    {
        const int size = 16;
        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        // Enable anti-aliasing for smooth text
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Fill with white background
        graphics.Clear(Color.White);

        // Draw blue gradient background
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, 0, size, size),
            Color.FromArgb(70, 130, 180),  // Steel blue
            Color.FromArgb(30, 80, 140),   // Darker blue
            45f);
        graphics.FillRectangle(brush, 0, 0, size, size);

        // Draw "あ" character
        using var font = new Font("Yu Gothic UI", 9f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.White);

        var text = "あ";
        var textSize = graphics.MeasureString(text, font);
        var x = (size - textSize.Width) / 2;
        var y = (size - textSize.Height) / 2;

        graphics.DrawString(text, font, textBrush, x, y);

        // Convert bitmap to icon
        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);

        // Clone the icon so we can safely dispose the handle
        Icon clonedIcon = (Icon)icon.Clone();
        NativeMethods.DestroyIcon(hIcon);

        return clonedIcon;
    }

    private void RegisterHotKeys()
    {
        try
        {
            _hotKeyWindow?.Dispose();
            _hotKeyWindow = new HotKeyWindow();
            _trayIcon.ShowBalloonTip(1000, "IME Hotkey", 
                "ホットキーを登録しました", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"ホットキーの登録に失敗しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ReregisterHotKeys()
    {
        RegisterHotKeys();
    }

    private void ExitApplication()
    {
        if (_isExiting) return;
        _isExiting = true;

        _hotKeyWindow?.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hotKeyWindow?.Dispose();
            _trayIcon?.Dispose();
            _customIcon?.Dispose();
        }
        base.Dispose(disposing);
    }
}
