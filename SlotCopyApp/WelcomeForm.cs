using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace SlotCopyApp
{
    public class WelcomeForm : Form
    {
        public WelcomeForm()
        {
            this.Size = new Size(600, 550);
            this.MinimumSize = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Welcome to SlotCopy";
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.Padding = new Padding(20);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Fills available space
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Title
            Label title = new Label
            {
                Text = "SlotCopy is now running!",
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 160, 255),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainLayout.Controls.Add(title, 0, 0);

            // Subtitle
            Label subtitle = new Label
            {
                Text = "Your app is quietly active in the system tray (bottom right corner).",
                AutoSize = true,
                ForeColor = Color.LightGray,
                Margin = new Padding(0, 0, 0, 20)
            };
            mainLayout.Controls.Add(subtitle, 0, 1);

            // Guide Panel (Scrollable)
            Panel guidePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                AutoScroll = true,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 20)
            };

            Label guideText = new Label
            {
                Text = "QUICK GUIDE:\n\n" +
                       "1. To Copy:\n" +
                       "   Press Ctrl + C (to prepare data), then press Ctrl + [1-9] to save it to a slot.\n\n" +
                       "2. To Paste:\n" +
                       "   Hold Ctrl + V, then press the slot number [1-9] to paste the data.\n\n" +
                       "3. Always On:\n" +
                       "   You can enable 'Start with Windows' by right-clicking the tray icon.\n\n" +
                       "4. Changing History:\n" +
                       "   Your slots are automatically saved. To overwrite a slot, simply save new data into it.",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            
            // Re-adjust maximum size on resize to wrap dynamically
            guidePanel.Resize += (s, e) => { guideText.MaximumSize = new Size(guidePanel.ClientSize.Width - 30, 0); };

            guidePanel.Controls.Add(guideText);
            mainLayout.Controls.Add(guidePanel, 0, 2);

            // Button layout
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            Button gotItButton = new Button
            {
                Text = "Awesome, let's go!",
                Size = new Size(200, 45),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            gotItButton.FlatAppearance.BorderSize = 0;
            gotItButton.Click += (s, e) => this.Close();
            
            LinkLabel link = new LinkLabel
            {
                Text = "Need help? View documentation",
                AutoSize = true,
                LinkColor = Color.FromArgb(0, 160, 255),
                ActiveLinkColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            link.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/DDurgeshmahajan/Slot-Copy-App") { UseShellExecute = true });
            
            bottomPanel.Controls.Add(gotItButton);
            bottomPanel.Controls.Add(link);
            
            // Manually position link to right side
            bottomPanel.Resize += (s, e) => { link.Location = new Point(bottomPanel.Width - link.Width, 12); };
            gotItButton.Location = new Point(0, 0);

            mainLayout.Controls.Add(bottomPanel, 0, 3);
            
            this.Controls.Add(mainLayout);
        }
    }
}
