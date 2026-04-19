using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ARPControl
{
    public class DynamicBoostForm : Form
    {
        private readonly List<PowerPlanInfo> _plans;
        private readonly AppSettings _settings;

        private CheckBox chkEnabled = null!;
        private ComboBox cmbActive = null!;
        private ComboBox cmbIdle = null!;
        private NumericUpDown numTimeout = null!;
        private CheckBox chkDisableOnBattery = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        public DynamicBoostForm(List<PowerPlanInfo> plans, AppSettings settings)
        {
            _plans = plans;
            _settings = settings;
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            Text = "ARPControl Dynamic Boost Settings";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(620, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var leftPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(190, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblDescTitle = new Label
            {
                Text = "Description",
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = "Dynamic Boost switches the\nactive power plan based on\nwhether the PC is active or idle.",
                Location = new Point(10, 35),
                Size = new Size(165, 100)
            };

            leftPanel.Controls.Add(lblDescTitle);
            leftPanel.Controls.Add(lblDesc);

            chkEnabled = new CheckBox
            {
                Text = "Dynamic Boost Enabled",
                Location = new Point(355, 20),
                AutoSize = true
            };

            var lblActive = new Label
            {
                Text = "When PC is ACTIVE use:",
                Location = new Point(230, 60),
                AutoSize = true
            };

            cmbActive = new ComboBox
            {
                Location = new Point(380, 55),
                Size = new Size(220, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblIdle = new Label
            {
                Text = "When PC is IDLE use:",
                Location = new Point(240, 100),
                AutoSize = true
            };

            cmbIdle = new ComboBox
            {
                Location = new Point(380, 95),
                Size = new Size(220, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblTimeout = new Label
            {
                Text = "Idle Timeout (seconds):",
                Location = new Point(330, 140),
                AutoSize = true
            };

            numTimeout = new NumericUpDown
            {
                Location = new Point(500, 136),
                Size = new Size(100, 24),
                Minimum = 1,
                Maximum = 3600
            };

            chkDisableOnBattery = new CheckBox
            {
                Text = "Disable when on battery power (DC)",
                Location = new Point(355, 180),
                AutoSize = true
            };

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(430, 230),
                Size = new Size(75, 30)
            };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(520, 230),
                Size = new Size(75, 30)
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(leftPanel);
            Controls.Add(chkEnabled);
            Controls.Add(lblActive);
            Controls.Add(cmbActive);
            Controls.Add(lblIdle);
            Controls.Add(cmbIdle);
            Controls.Add(lblTimeout);
            Controls.Add(numTimeout);
            Controls.Add(chkDisableOnBattery);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            cmbActive.DataSource = _plans.ToList();
            cmbIdle.DataSource = _plans.ToList();

            chkEnabled.Checked = _settings.DynamicBoostEnabled;
            chkDisableOnBattery.Checked = _settings.DisableDynamicBoostOnBattery;
            numTimeout.Value = _settings.DynamicBoostIdleTimeoutSeconds;

            SetComboByGuid(cmbActive, _settings.DynamicBoostActivePlanGuid);
            SetComboByGuid(cmbIdle, _settings.DynamicBoostIdlePlanGuid);
        }

        private void SetComboByGuid(ComboBox combo, string guid)
        {
            if (string.IsNullOrWhiteSpace(guid)) return;

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is PowerPlanInfo plan &&
                    plan.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            _settings.DynamicBoostEnabled = chkEnabled.Checked;
            _settings.DisableDynamicBoostOnBattery = chkDisableOnBattery.Checked;
            _settings.DynamicBoostIdleTimeoutSeconds = (int)numTimeout.Value;

            if (cmbActive.SelectedItem is PowerPlanInfo activePlan)
                _settings.DynamicBoostActivePlanGuid = activePlan.Guid;

            if (cmbIdle.SelectedItem is PowerPlanInfo idlePlan)
                _settings.DynamicBoostIdlePlanGuid = idlePlan.Guid;

            DialogResult = DialogResult.OK;
        }
    }
}