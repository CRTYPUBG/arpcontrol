using System;
using System.Drawing;
using System.Windows.Forms;

namespace ARPControl
{
    public class EventLogForm : Form
    {
        private DataGridView grid = null!;
        private Button btnRefresh = null!;
        private Button btnOk = null!;

        public EventLogForm()
        {
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            Text = "Recent Power Plan Events";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            grid = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(720, 320),
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Time",
                HeaderText = "Time",
                Width = 140
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Process",
                HeaderText = "Process",
                Width = 160
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PowerProfile",
                HeaderText = "Power profile",
                Width = 180
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Details",
                HeaderText = "Details",
                Width = 220
            });

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(12, 345),
                Size = new Size(80, 30)
            };
            btnRefresh.Click += (s, e) => LoadData();

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(650, 345),
                Size = new Size(80, 30)
            };
            btnOk.Click += (s, e) => Close();

            Controls.Add(grid);
            Controls.Add(btnRefresh);
            Controls.Add(btnOk);
        }

        private void LoadData()
        {
            grid.DataSource = null;
            grid.DataSource = EventLogger.GetAll();
        }
    }
}