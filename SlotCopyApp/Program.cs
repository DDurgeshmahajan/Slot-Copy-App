namespace SlotCopyApp;

using Microsoft.Win32;


static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using (TrayContext context = new TrayContext())
        {
            // Capture the UI thread context after TrayContext creates a dummy form handle
            OSD.Initialize(SynchronizationContext.Current);
            Application.Run(context);
        }
    }
}



public class TrayContext : ApplicationContext
{
    private NotifyIcon _trayIcon;
    private GlobalKeyboardHook _hook;
    private Form _dummyForm;

    public static bool IsStartupEnabled()
    {
        string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, false))
        {
            if (key == null) return false;
            string value = key.GetValue("SlotCopyApp") as string;
            return value == Application.ExecutablePath;
        }
    }

    public static void SetStartup(bool startWithWindows)
    {
        string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true))
        {
            if (startWithWindows)
            {
                key.SetValue("SlotCopyApp", Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue("SlotCopyApp", false);
            }
        }
    }

    public TrayContext()
    {
        _dummyForm = new Form { Visible = false, ShowInTaskbar = false };
        _dummyForm.CreateControl();

        _hook = new GlobalKeyboardHook();

        _trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true,
            Text = "Slot Copy Manager"
        };

        bool isStartup = IsStartupEnabled();

        var startupMenuItem = new ToolStripMenuItem("Start with Windows");
        startupMenuItem.CheckOnClick = true;
        startupMenuItem.Checked = isStartup;
        startupMenuItem.CheckedChanged += (s, e) => SetStartup(startupMenuItem.Checked);

        _trayIcon.ContextMenuStrip.Items.Add(startupMenuItem);
        _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) =>
        {
            _hook.Unhook();
            _trayIcon.Visible = false;
            Application.Exit();
        });

        // Sync startup path in case the app was moved to a different directory
        if (isStartup) SetStartup(true);
    }
}