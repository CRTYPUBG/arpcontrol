using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ARPControl
{
    public partial class Form1 : MaterialForm
    {
        private readonly PowerPlanService _powerService = new();
        private List<PowerPlanInfo> _plans = new();
        private AppSettings _settings = new();

        private MaterialCard leftCard = null!;
        private MaterialCard rightCard = null!;

        private ComboBox cmbProfiles = null!;
        private Button btnMakeActive = null!;
        private Button btnApply = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private Button btnShowAdvanced = null!;
        private Button btnSmartTune = null!;
        private Button btnCreateProfile = null!;

        private CheckBox chkReapply = null!;
        private LinkLabel lnkRestoreDefaults = null!;
        private Panel advancedPanel = null!;

        private Label lblStatusPlan = null!;
        private Label lblStatusPowerMode = null!;
        private Label lblStatusFreq = null!;
        private Label lblStatusCores = null!;
        private Panel cpuGraphPanel = null!;

        private RadioButton rbAcParkOn = null!;
        private RadioButton rbAcParkOff = null!;
        private TextBox txtAcPark = null!;
        private RadioButton rbAcFreqOn = null!;
        private RadioButton rbAcFreqOff = null!;
        private TextBox txtAcFreq = null!;

        private RadioButton rbDcParkOn = null!;
        private RadioButton rbDcParkOff = null!;
        private TextBox txtDcPark = null!;
        private RadioButton rbDcFreqOn = null!;
        private RadioButton rbDcFreqOff = null!;
        private TextBox txtDcFreq = null!;

        private ToolStripMenuItem miStartAtLogin = null!;
        private ToolStripMenuItem miDynamicBoostEnabled = null!;
        private ToolStripMenuItem miNotifyOnPowerProfileChange = null!;
        private ToolStripMenuItem miAlwaysShowHighPerformance = null!;
        private ToolStripMenuItem miAlwaysShowEfficiencyClassSelection = null!;
        private ToolStripMenuItem miPeriodicallyCheckForUpdates = null!;
        private ToolStripMenuItem miIncludeBetas = null!;

        private System.Windows.Forms.Timer statusTimer = null!;
        private System.Windows.Forms.Timer dynamicBoostTimer = null!;
        private string _lastObservedPlanGuid = "";

        public Form1()
        {
            InitializeComponent();
            _settings = AppPaths.LoadSettings();

            SetupMaterial();
            BuildUi();
            LoadPlans();
            LoadSettingsToUi();
            RefreshStatus();
            StartStatusTimer();
            StartDynamicBoostTimer();
            ReapplyIfNeeded();
        }

        private void SetupMaterial()
        {
            var manager = MaterialSkinManager.Instance;
            manager.AddFormToManage(this);
            manager.Theme = MaterialSkinManager.Themes.DARK;
            manager.ColorScheme = new ColorScheme(
                Primary.BlueGrey900,
                Primary.Grey900,
                Primary.BlueGrey500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            Text = "ARPControl";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1140, 720);
            MinimumSize = new Size(1140, 720);
            Sizable = false;
        }

        private void BuildUi()
        {
            BuildMenu();

            leftCard = new MaterialCard
            {
                Size = new Size(740, 540),
                Location = new Point(18, 78),
                Padding = new Padding(14)
            };

            rightCard = new MaterialCard
            {
                Size = new Size(330, 540),
                Location = new Point(775, 78),
                Padding = new Padding(14)
            };

            BuildLeftPanel();
            BuildRightPanel();
            BuildBottomButtons();

            Controls.Add(leftCard);
            Controls.Add(rightCard);
        }

        private void BuildMenu()
        {
            var menu = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(36, 36, 36),
                ForeColor = Color.White
            };

            var main = new ToolStripMenuItem("Main");
            var miClose = new ToolStripMenuItem("Close", null, (s, e) => Close());
            var activeProfile = new ToolStripMenuItem("Active Power Profile");
            main.DropDownItems.Add(miClose);
            main.DropDownItems.Add(activeProfile);
            main.DropDownItems.Add(new ToolStripSeparator());
            main.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()));

            var tools = new ToolStripMenuItem("Tools");
            tools.DropDownItems.Add(new ToolStripMenuItem("Show Power Plan Events", null, (s, e) =>
            {
                using var frm = new EventLogForm();
                frm.ShowDialog(this);
            }));

            var settings = new ToolStripMenuItem("Settings");

            miStartAtLogin = new ToolStripMenuItem("Start at Login");
            miStartAtLogin.Click += (s, e) =>
            {
                miStartAtLogin.Checked = !miStartAtLogin.Checked;
                _settings.StartAtLogin = miStartAtLogin.Checked;
                StartupManager.SetEnabled(_settings.StartAtLogin);
                SaveSettings();
            };

            miDynamicBoostEnabled = new ToolStripMenuItem("Dynamic Boost Enabled");
            miDynamicBoostEnabled.Click += (s, e) =>
            {
                miDynamicBoostEnabled.Checked = !miDynamicBoostEnabled.Checked;
                _settings.DynamicBoostEnabled = miDynamicBoostEnabled.Checked;
                SaveSettings();
            };

            var miDynamicBoostSettings = new ToolStripMenuItem("Dynamic Boost Settings");
            miDynamicBoostSettings.Click += (s, e) => OpenDynamicBoostSettings();

            var idleTimeout = new ToolStripMenuItem("Idle Timeout");
            foreach (int sec in new[] { 10, 30, 60, 300, 900, 1800 })
            {
                var item = new ToolStripMenuItem(FormatSeconds(sec));
                item.Tag = sec;
                item.Click += IdleTimeout_Click;
                idleTimeout.DropDownItems.Add(item);
            }

            miNotifyOnPowerProfileChange = new ToolStripMenuItem("Notify on Power Profile Change");
            miNotifyOnPowerProfileChange.Click += (s, e) =>
            {
                miNotifyOnPowerProfileChange.Checked = !miNotifyOnPowerProfileChange.Checked;
                _settings.NotifyOnPowerProfileChange = miNotifyOnPowerProfileChange.Checked;
                SaveSettings();
            };

            var experimental = new ToolStripMenuItem("Experimental");
            miAlwaysShowHighPerformance = new ToolStripMenuItem("Always Show High Performance");
            miAlwaysShowHighPerformance.Click += ToggleExperimental_Click;
            miAlwaysShowEfficiencyClassSelection = new ToolStripMenuItem("Always Show Efficiency Class Selection");
            miAlwaysShowEfficiencyClassSelection.Click += ToggleExperimental_Click;
            experimental.DropDownItems.Add(miAlwaysShowHighPerformance);
            experimental.DropDownItems.Add(miAlwaysShowEfficiencyClassSelection);

            var updates = new ToolStripMenuItem("Updates");
            miPeriodicallyCheckForUpdates = new ToolStripMenuItem("Periodically Check for Updates");
            miPeriodicallyCheckForUpdates.Click += ToggleUpdates_Click;
            miIncludeBetas = new ToolStripMenuItem("Include Betas");
            miIncludeBetas.Click += ToggleUpdates_Click;
            updates.DropDownItems.Add(miPeriodicallyCheckForUpdates);
            updates.DropDownItems.Add(miIncludeBetas);

            settings.DropDownItems.Add(miStartAtLogin);
            settings.DropDownItems.Add(new ToolStripSeparator());
            settings.DropDownItems.Add(miDynamicBoostEnabled);
            settings.DropDownItems.Add(miDynamicBoostSettings);
            settings.DropDownItems.Add(idleTimeout);
            settings.DropDownItems.Add(new ToolStripSeparator());
            settings.DropDownItems.Add(miNotifyOnPowerProfileChange);
            settings.DropDownItems.Add(new ToolStripSeparator());
            settings.DropDownItems.Add(experimental);
            settings.DropDownItems.Add(updates);

            var help = new ToolStripMenuItem("Help");
            help.DropDownItems.Add(new ToolStripMenuItem("Check for Updates", null, (s, e) =>
                MessageBox.Show("Þu an yerel sürüm çalýþýyor. Online update servisi baðlý deðil.", "ARPControl")));
            help.DropDownItems.Add(new ToolStripMenuItem("Change License Code", null, (s, e) =>
                MessageBox.Show("Lisans sistemi örnek sürümde devre dýþý.", "ARPControl")));
            help.DropDownItems.Add(new ToolStripMenuItem("About", null, (s, e) =>
                MessageBox.Show("ARPControl v2\nPower plan manager for Windows\n(c) 2026", "About")));

            menu.Items.Add(main);
            menu.Items.Add(tools);
            menu.Items.Add(settings);
            menu.Items.Add(help);

            Controls.Add(menu);

            void RefreshActivePlanMenu()
            {
                activeProfile.DropDownItems.Clear();
                foreach (var p in _plans)
                {
                    var item = new ToolStripMenuItem(p.Name)
                    {
                        Checked = p.Guid.Equals(_powerService.GetActivePlanGuid(), StringComparison.OrdinalIgnoreCase),
                        Tag = p.Guid
                    };
                    item.Click += (s, e) =>
                    {
                        _powerService.SetActivePlan((string)((ToolStripMenuItem)s!).Tag!);
                        RefreshStatus();
                    };
                    activeProfile.DropDownItems.Add(item);
                }
            }

            this.Shown += (s, e) => RefreshActivePlanMenu();
        }

        private void BuildLeftPanel()
        {
            var lblCpuSettings = CreateWhiteLabel("CPU Settings for Power Profile", new Point(20, 20), true);

            cmbProfiles = new ComboBox
            {
                Location = new Point(20, 50),
                Size = new Size(360, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            btnMakeActive = CreateButton("Make Active", new Point(395, 47), new Size(110, 32));
            btnMakeActive.Click += BtnMakeActive_Click;

            var acGroup = CreatePowerModePanel(
                "Plugged In (AC)",
                new Point(20, 110),
                out rbAcParkOn, out rbAcParkOff, out txtAcPark,
                out rbAcFreqOn, out rbAcFreqOff, out txtAcFreq
            );

            var dcGroup = CreatePowerModePanel(
                "On Battery (DC)",
                new Point(380, 110),
                out rbDcParkOn, out rbDcParkOff, out txtDcPark,
                out rbDcFreqOn, out rbDcFreqOff, out txtDcFreq
            );

            btnShowAdvanced = CreateButton("Hide Advanced", new Point(20, 400), new Size(120, 32));
            btnShowAdvanced.Click += (s, e) =>
            {
                advancedPanel.Visible = !advancedPanel.Visible;
                btnShowAdvanced.Text = advancedPanel.Visible ? "Hide Advanced" : "Show Advanced";
            };

            lnkRestoreDefaults = new LinkLabel
            {
                Text = "Restore defaults",
                Location = new Point(320, 408),
                AutoSize = true,
                LinkColor = Color.DeepSkyBlue,
                ActiveLinkColor = Color.LightSkyBlue,
                VisitedLinkColor = Color.DeepSkyBlue
            };
            lnkRestoreDefaults.Click += (s, e) => RestoreDefaults();

            btnApply = CreateButton("Apply", new Point(620, 400), new Size(90, 32));
            btnApply.Click += BtnApply_Click;

            chkReapply = new CheckBox
            {
                Text = "Reapply settings for this power profile when ARPControl starts",
                Location = new Point(20, 445),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            chkReapply.CheckedChanged += (s, e) =>
            {
                _settings.ReapplyOnStartup = chkReapply.Checked;
                SaveSettings();
            };

            advancedPanel = new Panel
            {
                Location = new Point(20, 475),
                Size = new Size(690, 45),
                BackColor = Color.Transparent
            };

            btnSmartTune = CreateButton("Smart AutoTune", new Point(0, 0), new Size(120, 32));
            btnSmartTune.Click += (s, e) => ApplySmartAutoTune();

            btnCreateProfile = CreateButton("Create Custom Profile", new Point(140, 0), new Size(145, 32));
            btnCreateProfile.Click += (s, e) => CreateCustomProfile();

            var btnEvents = CreateButton("Session Events", new Point(305, 0), new Size(120, 32));
            btnEvents.Click += (s, e) =>
            {
                using var frm = new EventLogForm();
                frm.ShowDialog(this);
            };

            advancedPanel.Controls.Add(btnSmartTune);
            advancedPanel.Controls.Add(btnCreateProfile);
            advancedPanel.Controls.Add(btnEvents);

            leftCard.Controls.Add(lblCpuSettings);
            leftCard.Controls.Add(cmbProfiles);
            leftCard.Controls.Add(btnMakeActive);
            leftCard.Controls.Add(acGroup);
            leftCard.Controls.Add(dcGroup);
            leftCard.Controls.Add(btnShowAdvanced);
            leftCard.Controls.Add(lnkRestoreDefaults);
            leftCard.Controls.Add(btnApply);
            leftCard.Controls.Add(chkReapply);
            leftCard.Controls.Add(advancedPanel);
        }

        private void BuildRightPanel()
        {
            rightCard.Controls.Add(CreateWhiteLabel("Current System Power Status", new Point(20, 20), true));

            lblStatusPlan = CreateWhiteLabel("Unknown Plan", new Point(20, 60), true, 14F);
            lblStatusPowerMode = CreateWhiteLabel("AC", new Point(20, 102));
            lblStatusFreq = CreateWhiteLabel("N/A", new Point(20, 140), true, 20F);
            rightCard.Controls.Add(CreateGrayLabel("current CPU frequency", new Point(20, 182)));
            lblStatusCores = CreateWhiteLabel("0 of 0 cores", new Point(20, 220), true, 18F);

            rightCard.Controls.Add(lblStatusPlan);
            rightCard.Controls.Add(lblStatusPowerMode);
            rightCard.Controls.Add(lblStatusFreq);
            rightCard.Controls.Add(lblStatusCores);
            rightCard.Controls.Add(CreateWhiteLabel("CPU", new Point(20, 285), true));

            cpuGraphPanel = CreateCpuGraphPanel();
            cpuGraphPanel.Location = new Point(20, 315);
            rightCard.Controls.Add(cpuGraphPanel);
        }

        private void BuildBottomButtons()
        {
            btnOk = CreateButton("OK", new Point(900, 630), new Size(90, 34));
            btnOk.Click += (s, e) => Close();

            btnCancel = CreateButton("Cancel", new Point(1000, 630), new Size(90, 34));
            btnCancel.Click += (s, e) => Close();

            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private Panel CreatePowerModePanel(
            string title,
            Point location,
            out RadioButton rbParkOn,
            out RadioButton rbParkOff,
            out TextBox txtPark,
            out RadioButton rbFreqOn,
            out RadioButton rbFreqOff,
            out TextBox txtFreq)
        {
            var panel = new Panel
            {
                Location = location,
                Size = new Size(340, 240),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(16, 22, 30)
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.White,
                BackColor = panel.BackColor,
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblParking = CreatePanelLabel("Parking", new Point(12, 42), panel.BackColor);
            rbParkOn = CreatePanelRadio("On", new Point(135, 39), panel.BackColor);
            rbParkOff = CreatePanelRadio("Off", new Point(190, 39), panel.BackColor, true);
            txtPark = CreatePanelTextBox("100", new Point(250, 38));
            var lblPct1 = CreatePanelLabel("%", new Point(294, 42), panel.BackColor);

            var lblFreq = CreatePanelLabel("Freq Scaling", new Point(12, 82), panel.BackColor);
            rbFreqOn = CreatePanelRadio("On", new Point(135, 79), panel.BackColor);
            rbFreqOff = CreatePanelRadio("Off", new Point(190, 79), panel.BackColor, true);
            txtFreq = CreatePanelTextBox("100", new Point(250, 78));
            var lblPct2 = CreatePanelLabel("%", new Point(294, 82), panel.BackColor);

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblParking);
            panel.Controls.Add(rbParkOn);
            panel.Controls.Add(rbParkOff);
            panel.Controls.Add(txtPark);
            panel.Controls.Add(lblPct1);
            panel.Controls.Add(lblFreq);
            panel.Controls.Add(rbFreqOn);
            panel.Controls.Add(rbFreqOff);
            panel.Controls.Add(txtFreq);
            panel.Controls.Add(lblPct2);

            return panel;
        }

        private void LoadPlans()
        {
            _plans = _powerService.GetPlans();

            cmbProfiles.Items.Clear();
            foreach (var plan in _plans)
                cmbProfiles.Items.Add(plan.Name);

            string activeGuid = _powerService.GetActivePlanGuid();
            int index = _plans.FindIndex(x => x.Guid.Equals(activeGuid, StringComparison.OrdinalIgnoreCase));
            cmbProfiles.SelectedIndex = index >= 0 ? index : (_plans.Count > 0 ? 0 : -1);

            _lastObservedPlanGuid = activeGuid;
        }

        private void LoadSettingsToUi()
        {
            miStartAtLogin.Checked = StartupManager.IsEnabled() || _settings.StartAtLogin;
            miDynamicBoostEnabled.Checked = _settings.DynamicBoostEnabled;
            miNotifyOnPowerProfileChange.Checked = _settings.NotifyOnPowerProfileChange;
            miAlwaysShowHighPerformance.Checked = _settings.AlwaysShowHighPerformance;
            miAlwaysShowEfficiencyClassSelection.Checked = _settings.AlwaysShowEfficiencyClassSelection;
            miPeriodicallyCheckForUpdates.Checked = _settings.PeriodicallyCheckForUpdates;
            miIncludeBetas.Checked = _settings.IncludeBetas;

            chkReapply.Checked = _settings.ReapplyOnStartup;

            rbAcParkOn.Checked = _settings.AcParkingEnabled;
            rbAcParkOff.Checked = !_settings.AcParkingEnabled;
            txtAcPark.Text = _settings.AcParkingPercent.ToString();

            rbDcParkOn.Checked = _settings.DcParkingEnabled;
            rbDcParkOff.Checked = !_settings.DcParkingEnabled;
            txtDcPark.Text = _settings.DcParkingPercent.ToString();

            rbAcFreqOn.Checked = _settings.AcFreqScalingEnabled;
            rbAcFreqOff.Checked = !_settings.AcFreqScalingEnabled;
            txtAcFreq.Text = _settings.AcFreqPercent.ToString();

            rbDcFreqOn.Checked = _settings.DcFreqScalingEnabled;
            rbDcFreqOff.Checked = !_settings.DcFreqScalingEnabled;
            txtDcFreq.Text = _settings.DcFreqPercent.ToString();

            if (!string.IsNullOrWhiteSpace(_settings.SelectedPlanGuid))
            {
                int idx = _plans.FindIndex(x => x.Guid.Equals(_settings.SelectedPlanGuid, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                    cmbProfiles.SelectedIndex = idx;
            }
        }

        private void SaveSettingsFromUi()
        {
            _settings.StartAtLogin = miStartAtLogin.Checked;
            _settings.DynamicBoostEnabled = miDynamicBoostEnabled.Checked;
            _settings.NotifyOnPowerProfileChange = miNotifyOnPowerProfileChange.Checked;
            _settings.AlwaysShowHighPerformance = miAlwaysShowHighPerformance.Checked;
            _settings.AlwaysShowEfficiencyClassSelection = miAlwaysShowEfficiencyClassSelection.Checked;
            _settings.PeriodicallyCheckForUpdates = miPeriodicallyCheckForUpdates.Checked;
            _settings.IncludeBetas = miIncludeBetas.Checked;
            _settings.ReapplyOnStartup = chkReapply.Checked;

            _settings.AcParkingEnabled = rbAcParkOn.Checked;
            _settings.DcParkingEnabled = rbDcParkOn.Checked;
            _settings.AcFreqScalingEnabled = rbAcFreqOn.Checked;
            _settings.DcFreqScalingEnabled = rbDcFreqOn.Checked;

            _settings.AcParkingPercent = ParsePercent(txtAcPark.Text, 100);
            _settings.DcParkingPercent = ParsePercent(txtDcPark.Text, 100);
            _settings.AcFreqPercent = ParsePercent(txtAcFreq.Text, 100);
            _settings.DcFreqPercent = ParsePercent(txtDcFreq.Text, 100);

            if (cmbProfiles.SelectedIndex >= 0 && cmbProfiles.SelectedIndex < _plans.Count)
                _settings.SelectedPlanGuid = _plans[cmbProfiles.SelectedIndex].Guid;
        }

        private void SaveSettings()
        {
            SaveSettingsFromUi();
            AppPaths.SaveSettings(_settings);
        }

        private void ReapplyIfNeeded()
        {
            if (!_settings.ReapplyOnStartup)
                return;

            if (string.IsNullOrWhiteSpace(_settings.SelectedPlanGuid))
                return;

            try
            {
                _powerService.ApplyProcessorSettings(
                    _settings.SelectedPlanGuid,
                    _settings.AcParkingPercent,
                    _settings.DcParkingPercent,
                    _settings.AcFreqPercent,
                    _settings.DcFreqPercent,
                    _settings.AcParkingEnabled,
                    _settings.DcParkingEnabled,
                    _settings.AcFreqScalingEnabled,
                    _settings.DcFreqScalingEnabled);

                EventLogger.Add("Startup", _powerService.GetActivePlanName(), "Saved settings reapplied");
            }
            catch
            {
            }
        }

        private void BtnMakeActive_Click(object? sender, EventArgs e)
        {
            if (cmbProfiles.SelectedIndex < 0 || cmbProfiles.SelectedIndex >= _plans.Count)
                return;

            var selected = _plans[cmbProfiles.SelectedIndex];
            _powerService.SetActivePlan(selected.Guid);
            SaveSettings();
            RefreshStatus();

            EventLogger.Add("User", selected.Name, "Plan activated");

            if (_settings.NotifyOnPowerProfileChange)
                MessageBox.Show($"Aktif plan deðiþti:\n{selected.Name}", "ARPControl");
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (cmbProfiles.SelectedIndex < 0 || cmbProfiles.SelectedIndex >= _plans.Count)
            {
                MessageBox.Show("Bir güç planý seç.", "ARPControl");
                return;
            }

            if (!ValidateAllInputs())
            {
                MessageBox.Show("Tüm yüzde alanlarý 0-100 arasýnda olmalý.", "ARPControl");
                return;
            }

            var selected = _plans[cmbProfiles.SelectedIndex];

            _powerService.ApplyProcessorSettings(
                selected.Guid,
                ParsePercent(txtAcPark.Text, 100),
                ParsePercent(txtDcPark.Text, 100),
                ParsePercent(txtAcFreq.Text, 100),
                ParsePercent(txtDcFreq.Text, 100),
                rbAcParkOn.Checked,
                rbDcParkOn.Checked,
                rbAcFreqOn.Checked,
                rbDcFreqOn.Checked
            );

            SaveSettings();
            RefreshStatus();

            EventLogger.Add("User", selected.Name, "Processor settings applied");

            MessageBox.Show("Ayarlar uygulandý.", "ARPControl");
        }

        private void ApplySmartAutoTune()
        {
            bool online = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;

            if (online)
            {
                rbAcParkOff.Checked = true;
                txtAcPark.Text = "100";
                rbAcFreqOff.Checked = true;
                txtAcFreq.Text = "100";

                rbDcParkOn.Checked = true;
                txtDcPark.Text = "35";
                rbDcFreqOn.Checked = true;
                txtDcFreq.Text = "65";
            }
            else
            {
                rbAcParkOn.Checked = true;
                txtAcPark.Text = "60";
                rbAcFreqOn.Checked = true;
                txtAcFreq.Text = "80";

                rbDcParkOn.Checked = true;
                txtDcPark.Text = "25";
                rbDcFreqOn.Checked = true;
                txtDcFreq.Text = "50";
            }

            EventLogger.Add("Smart AutoTune", _powerService.GetActivePlanName(), "Recommended values applied to UI");
            MessageBox.Show("Smart AutoTune deðerleri dolduruldu.", "ARPControl");
        }

        private void CreateCustomProfile()
        {
            string name = $"ARP Custom {DateTime.Now:HHmmss}";
            string guid = _powerService.DuplicateActivePlan(name);

            if (string.IsNullOrWhiteSpace(guid))
            {
                MessageBox.Show("Yeni profil oluþturulamadý.", "ARPControl");
                return;
            }

            EventLogger.Add("Profile Creator", name, "Custom profile duplicated from active plan");

            LoadPlans();

            int idx = _plans.FindIndex(x => x.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                cmbProfiles.SelectedIndex = idx;

            MessageBox.Show($"Yeni profil oluþturuldu:\n{name}", "ARPControl");
        }

        private void OpenDynamicBoostSettings()
        {
            using var frm = new DynamicBoostForm(_plans, _settings);
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                SaveSettings();
            }
        }

        private void IdleTimeout_Click(object? sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in ((ToolStripMenuItem)((ToolStripMenuItem)sender!).OwnerItem!).DropDownItems)
                item.Checked = false;

            var clicked = (ToolStripMenuItem)sender!;
            clicked.Checked = true;

            _settings.DynamicBoostIdleTimeoutSeconds = (int)clicked.Tag!;
            SaveSettings();
        }

        private void ToggleExperimental_Click(object? sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender!;
            item.Checked = !item.Checked;

            _settings.AlwaysShowHighPerformance = miAlwaysShowHighPerformance.Checked;
            _settings.AlwaysShowEfficiencyClassSelection = miAlwaysShowEfficiencyClassSelection.Checked;
            SaveSettings();
        }

        private void ToggleUpdates_Click(object? sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender!;
            item.Checked = !item.Checked;

            _settings.PeriodicallyCheckForUpdates = miPeriodicallyCheckForUpdates.Checked;
            _settings.IncludeBetas = miIncludeBetas.Checked;
            SaveSettings();
        }

        private void RestoreDefaults()
        {
            rbAcParkOff.Checked = true;
            rbDcParkOff.Checked = true;
            rbAcFreqOff.Checked = true;
            rbDcFreqOff.Checked = true;

            txtAcPark.Text = "100";
            txtDcPark.Text = "100";
            txtAcFreq.Text = "100";
            txtDcFreq.Text = "100";

            EventLogger.Add("User", _powerService.GetActivePlanName(), "Default values restored");
        }

        private bool ValidateAllInputs()
        {
            return IsValidPercent(txtAcPark.Text)
                && IsValidPercent(txtDcPark.Text)
                && IsValidPercent(txtAcFreq.Text)
                && IsValidPercent(txtDcFreq.Text);
        }

        private bool IsValidPercent(string text)
        {
            return int.TryParse(text, out int v) && v >= 0 && v <= 100;
        }

        private int ParsePercent(string text, int fallback)
        {
            return int.TryParse(text, out int v) ? Math.Max(0, Math.Min(100, v)) : fallback;
        }

        private void RefreshStatus()
        {
            lblStatusPlan.Text = _powerService.GetActivePlanName();
            lblStatusPowerMode.Text = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online ? "AC" : "Battery";
            lblStatusFreq.Text = GetCpuFrequencyText();
            lblStatusCores.Text = $"{Environment.ProcessorCount} of {Environment.ProcessorCount} cores";
            cpuGraphPanel.Invalidate();
        }

        private void StartStatusTimer()
        {
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1500;
            statusTimer.Tick += (s, e) =>
            {
                RefreshStatus();

                string currentGuid = _powerService.GetActivePlanGuid();
                if (!string.Equals(currentGuid, _lastObservedPlanGuid, StringComparison.OrdinalIgnoreCase))
                {
                    _lastObservedPlanGuid = currentGuid;
                    EventLogger.Add("System", _powerService.GetActivePlanName(), "Detected active plan change");
                }
            };
            statusTimer.Start();
        }

        private void StartDynamicBoostTimer()
        {
            dynamicBoostTimer = new System.Windows.Forms.Timer();
            dynamicBoostTimer.Interval = 2000;
            dynamicBoostTimer.Tick += (s, e) => HandleDynamicBoost();
            dynamicBoostTimer.Start();
        }

        private void HandleDynamicBoost()
        {
            if (!_settings.DynamicBoostEnabled)
                return;

            if (_settings.DisableDynamicBoostOnBattery &&
                SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online)
                return;

            int idleSeconds = GetIdleSeconds();
            string targetGuid = idleSeconds >= _settings.DynamicBoostIdleTimeoutSeconds
                ? _settings.DynamicBoostIdlePlanGuid
                : _settings.DynamicBoostActivePlanGuid;

            if (string.IsNullOrWhiteSpace(targetGuid))
                return;

            string activeGuid = _powerService.GetActivePlanGuid();
            if (targetGuid.Equals(activeGuid, StringComparison.OrdinalIgnoreCase))
                return;

            var plan = _powerService.FindByGuid(_plans, targetGuid);
            _powerService.SetActivePlan(targetGuid);

            EventLogger.Add("Dynamic Boost", plan?.Name ?? targetGuid,
                idleSeconds >= _settings.DynamicBoostIdleTimeoutSeconds ? "Switched to IDLE plan" : "Switched to ACTIVE plan");
        }

        private string GetCpuFrequencyText()
        {
            try
            {
                using var counter = new PerformanceCounter("Processor Information", "Processor Frequency", "_Total");
                _ = counter.NextValue();
                System.Threading.Thread.Sleep(120);
                float mhz = counter.NextValue();

                if (mhz <= 0) return "N/A";
                return $"{mhz / 1000f:0.00} GHz";
            }
            catch
            {
                return "N/A";
            }
        }

        private Panel CreateCpuGraphPanel()
        {
            var panel = new Panel
            {
                Size = new Size(290, 180),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var gridPen = new Pen(Color.FromArgb(70, 70, 70));
                using var greenBrush = new SolidBrush(Color.LimeGreen);

                int width = panel.Width;
                int height = panel.Height;

                g.DrawLine(gridPen, 0, height / 2, width, height / 2);

                for (int i = 1; i < 6; i++)
                {
                    int x = i * width / 6;
                    g.DrawLine(gridPen, x, 0, x, height);
                }

                int t = DateTime.Now.Second % 6;
                int[] topBars = { 30 + t * 3, 18 + t * 2, 14 + t, 32 + t * 2, 12 + t, 18 + t * 2 };
                int[] bottomBars = { 9 + t, 12 + t, 10 + t, 18 + t * 2, 11 + t, 14 + t };

                int barWidth = 40;
                for (int i = 0; i < topBars.Length; i++)
                {
                    int x = 10 + i * 46;
                    g.FillRectangle(greenBrush, x, height / 2 - topBars[i], barWidth, topBars[i]);
                }

                for (int i = 0; i < bottomBars.Length; i++)
                {
                    int x = 10 + i * 46;
                    g.FillRectangle(greenBrush, x, height - bottomBars[i] - 8, barWidth, bottomBars[i]);
                }
            };

            return panel;
        }

        private Button CreateButton(string text, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(58, 58, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderColor = Color.Gray;
            return btn;
        }

        private Label CreateWhiteLabel(string text, Point location, bool bold = false, float size = 9F)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private Label CreateGrayLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F)
            };
        }

        private Label CreatePanelLabel(string text, Point location, Color back)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = back
            };
        }

        private RadioButton CreatePanelRadio(string text, Point location, Color back, bool isChecked = false)
        {
            return new RadioButton
            {
                Text = text,
                Location = location,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = back,
                Checked = isChecked
            };
        }

        private TextBox CreatePanelTextBox(string text, Point location)
        {
            return new TextBox
            {
                Text = text,
                Location = location,
                Size = new Size(40, 23)
            };
        }

        private static string FormatSeconds(int sec)
        {
            if (sec < 60) return $"{sec} Seconds";
            if (sec % 60 == 0) return $"{sec / 60} Minute" + (sec / 60 > 1 ? "s" : "");
            return $"{sec} Seconds";
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private int GetIdleSeconds()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            if (!GetLastInputInfo(ref info))
                return 0;

            uint idleTicks = unchecked((uint)Environment.TickCount - info.dwTime);
            return (int)(idleTicks / 1000);
        }
    }
}