namespace ARPControl
{
    public class AppSettings
    {
        public bool StartAtLogin { get; set; }
        public bool DynamicBoostEnabled { get; set; }
        public bool NotifyOnPowerProfileChange { get; set; } = true;
        public bool ReapplyOnStartup { get; set; } = true;

        public string DynamicBoostActivePlanGuid { get; set; } = "";
        public string DynamicBoostIdlePlanGuid { get; set; } = "";
        public int DynamicBoostIdleTimeoutSeconds { get; set; } = 10;
        public bool DisableDynamicBoostOnBattery { get; set; } = true;

        public string SelectedPlanGuid { get; set; } = "";

        public bool AcParkingEnabled { get; set; }
        public int AcParkingPercent { get; set; } = 100;

        public bool DcParkingEnabled { get; set; }
        public int DcParkingPercent { get; set; } = 100;

        public bool AcFreqScalingEnabled { get; set; }
        public int AcFreqPercent { get; set; } = 100;

        public bool DcFreqScalingEnabled { get; set; }
        public int DcFreqPercent { get; set; } = 100;

        public bool AlwaysShowHighPerformance { get; set; } = true;
        public bool AlwaysShowEfficiencyClassSelection { get; set; } = false;
        public bool PeriodicallyCheckForUpdates { get; set; } = true;
        public bool IncludeBetas { get; set; } = false;
    }
}