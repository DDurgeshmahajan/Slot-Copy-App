namespace SlotCopyApp;

using Microsoft.Win32;


static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Capture the UI thread context before running the app
        OSD.Initialize(SynchronizationContext.Current);

        using (TrayContext context = new TrayContext())
        {
            Application.Run(context);
        }
    }
}



public class TrayContext : ApplicationContext
{
    private NotifyIcon _trayIcon;
    private GlobalKeyboardHook _hook;
    private Form _dummyForm;


    public static void SetStartup(bool startWithWindows)
    {
        string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true);

        if (startWithWindows)
        {
            key.SetValue("SlotCopyApp", Application.ExecutablePath);
        }
        else
        {
            key.DeleteValue("SlotCopyApp", false);
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

        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) =>
        {
            _hook.Unhook();
            _trayIcon.Visible = false;
            Application.Exit();
        });
    }
}