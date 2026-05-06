namespace SlotCopyApp;

using Microsoft.Win32;


using System.Threading;

static class Program
{
    static Mutex mutex = new Mutex(true, "{SlotCopyApp-Unique-Mutex-123}");

    [STAThread]
    static void Main()
    {
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("SlotCopy is already running in the background!\n\nCheck your system tray (bottom right corner near the clock) for the icon.", "SlotCopy Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();

        using (TrayContext context = new TrayContext())
        {
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

        var slotsMenuItem = new ToolStripMenuItem("Slots Texts");
        slotsMenuItem.Click += (s, e) =>
        {
            var slots = _hook.GetSlots();
            SlotsListForm form = new SlotsListForm(slots);
            form.Show();
        };

        _trayIcon.ContextMenuStrip.Items.Add(slotsMenuItem);
        _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _trayIcon.ContextMenuStrip.Items.Add(startupMenuItem);
        _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) =>
        {
            _hook.Unhook();
            _trayIcon.Visible = false;
            Application.Exit();
        });

        if (isStartup) SetStartup(true);

        CheckFirstRun();
    }

    private void CheckFirstRun()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appData, "SlotCopyApp");
        System.IO.Directory.CreateDirectory(appFolder);
        string markerPath = Path.Combine(appFolder, ".has_run");

        if (!System.IO.File.Exists(markerPath))
        {
            System.IO.File.WriteAllText(markerPath, "1");
            WelcomeForm welcome = new WelcomeForm();
            welcome.Show();
        }
    }
}