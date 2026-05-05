using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SlotCopyApp
{
    public static class OSD
    {
        private static SynchronizationContext _uiContext;
        private static Form _osdForm;


        public static void Initialize(SynchronizationContext context)
        {
            _uiContext = context;
        }

        public static void Show(string message)
        {

            if (_uiContext == null) return;


            _uiContext.Post(_ =>
            {
                try
                {
                    _osdForm?.Close();

                    _osdForm = new Form
                    {
                        Size = new Size(220, 50),
                        FormBorderStyle = FormBorderStyle.None,
                        StartPosition = FormStartPosition.Manual,
                        TopMost = true,
                        ShowInTaskbar = false,
                        BackColor = Color.FromArgb(20, 20, 20),
                        Opacity = 0.85
                    };

                    Label lbl = new Label
                    {
                        Text = message,
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };

                    _osdForm.Controls.Add(lbl);


                    var area = Screen.PrimaryScreen.WorkingArea;
                    _osdForm.Location = new Point(area.Right - 230, area.Bottom - 60);

                    _osdForm.Show();


                    var timer = new System.Windows.Forms.Timer { Interval = 1500 };
                    timer.Tick += (s, e) =>
                    {
                        _osdForm?.Close();
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
                catch { /*fail silently*/ }
            }, null);
        }
    }
}