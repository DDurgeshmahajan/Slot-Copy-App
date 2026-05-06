using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SlotCopyApp
{
    public class SlotsListForm : Form
    {
        public SlotsListForm(Dictionary<int, string> slots)
        {
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.Manual;
            
            // Position near bottom right
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 10, workingArea.Bottom - this.Height - 10);
            
            this.Text = "Saved Slots";
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.Padding = new Padding(10);
            this.FormBorderStyle = FormBorderStyle.None; // Borderless for creative look
            this.TopMost = true;
            this.ShowInTaskbar = false;

            InitializeComponents(slots);
        }

        private void InitializeComponents(Dictionary<int, string> slots)
        {
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            FlowLayoutPanel layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            int safeWidth = this.ClientSize.Width - 50; // Safely avoid horizontal scrollbar

            Panel headerPanel = new Panel
            {
                Width = safeWidth,
                Height = 40,
                Margin = new Padding(0, 0, 0, 15)
            };

            Label title = new Label
            {
                Text = "Current Saved Slots",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 160, 255),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            
            Button closeButton = new Button
            {
                Text = "X",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(headerPanel.Width - 30, 0),
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();
            
            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(closeButton);
            layout.Controls.Add(headerPanel);

            if (slots == null || slots.Count == 0)
            {
                Label empty = new Label
                {
                    Text = "No slots saved yet.\nPress Ctrl+C then Ctrl+[1-9] to save.",
                    AutoSize = true,
                    ForeColor = Color.LightGray,
                    Margin = new Padding(0, 10, 0, 0)
                };
                layout.Controls.Add(empty);
            }
            else
            {
                foreach (var kvp in slots.OrderBy(k => k.Key))
                {
                    Panel slotPanel = new Panel
                    {
                        Width = safeWidth,
                        AutoSize = false,
                        BackColor = Color.FromArgb(45, 45, 45),
                        Margin = new Padding(0, 0, 0, 10),
                        Padding = new Padding(10)
                    };

                    Label slotHeader = new Label
                    {
                        Text = $"Slot {kvp.Key}",
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 160, 255),
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };
                    slotPanel.Controls.Add(slotHeader);

                    string previewText = kvp.Value;
                    
                    var lines = previewText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    if (lines.Length > 4)
                    {
                        previewText = string.Join(Environment.NewLine, lines.Take(4)) + "\n...";
                    }

                    if (previewText.Length > 200)
                    {
                        previewText = previewText.Substring(0, 200) + "...";
                    }

                    Label slotContent = new Label
                    {
                        Text = previewText,
                        AutoSize = true,
                        MaximumSize = new Size(slotPanel.Width - 20, 0),
                        Location = new Point(10, 35),
                        ForeColor = Color.LightGray
                    };
                    slotPanel.Controls.Add(slotContent);

                    slotPanel.Height = slotContent.Bottom + 10;
                    
                    layout.Controls.Add(slotPanel);
                }
            }

            mainPanel.Controls.Add(layout);
            this.Controls.Add(mainPanel);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Close();
        }
    }
}
