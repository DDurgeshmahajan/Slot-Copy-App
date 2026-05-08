using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SlotCopyApp
{
    public static class OSD
    {
        private static SynchronizationContext _uiContext;
        private static OSDForm _osdForm;
        private static System.Windows.Forms.Timer _timer;

        private class OSDForm : Form
        {
            public OSDForm()
            {
                this.DoubleBuffered = true;
            }

            protected override bool ShowWithoutActivation => true;
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT (Click-through)
                    cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                    cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                    return cp;
                }
            }
        }

        public static void Initialize(SynchronizationContext context)
        {
            _uiContext = context;
        }

        public static void Show(string message)
        {
            // Try to recover context if it was null (might happen if initialized too early)
            if (_uiContext == null)
            {
                _uiContext = SynchronizationContext.Current;
            }

            if (_uiContext == null) return;

            _uiContext.Post(_ =>
            {
                try
                {
                    if (_osdForm == null || _osdForm.IsDisposed)
                    {
                        _osdForm = new OSDForm
                        {
                            Size = new Size(220, 50),
                            FormBorderStyle = FormBorderStyle.None,
                            StartPosition = FormStartPosition.Manual,
                            TopMost = true,
                            ShowInTaskbar = false,
                            BackColor = Color.FromArgb(20, 20, 20),
                            Opacity = 0
                        };

                        Label lbl = new Label
                        {
                            Name = "MessageLabel",
                            Text = message,
                            ForeColor = Color.White,
                            BackColor = Color.Transparent,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Dock = DockStyle.Fill,
                            Font = new Font("Segoe UI", 11, FontStyle.Bold)
                        };

                        _osdForm.Controls.Add(lbl);
                        _osdForm.Show();
                    }

                    // Always update location in case screen resolution or primary screen changed
                    var area = Screen.PrimaryScreen.WorkingArea;
                    _osdForm.Location = new Point(area.Right - 230, area.Bottom - 60);
                    
                    _osdForm.Controls["MessageLabel"].Text = message;
                    _osdForm.Opacity = 0.85;
                    _osdForm.TopMost = true;
                    _osdForm.BringToFront();

                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }

                    _timer = new System.Windows.Forms.Timer { Interval = 1500 };
                    _timer.Tick += (s, e) =>
                    {
                        if (_osdForm != null && !_osdForm.IsDisposed)
                        {
                            _osdForm.Opacity = 0;
                        }
                        _timer.Stop();
                    };
                    _timer.Start();
                }
                catch 
                { 
                    // If something went wrong, let's try to reset the form for next time
                    if (_osdForm != null)
                    {
                        try { _osdForm.Dispose(); } catch { }
                        _osdForm = null;
                    }
                }
            }, null);
        }
    }
}