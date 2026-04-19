using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ARPControl
{
    public class PowerPlanService
    {
        private const string SUB_PROCESSOR = "54533251-82be-4824-96c1-47b60b740d00";
        private const string SETTING_MIN_PROC_STATE = "893dee8e-2bef-41e0-89c6-b55d0929964c";
        private const string SETTING_MAX_PROC_STATE = "bc5038f7-23e0-4960-96da-33abaf5935ec";
        private const string SETTING_CORE_PARKING_MIN_CORES = "0cc5b647-c1df-4637-891a-dec35c318583";

        public List<PowerPlanInfo> GetPlans()
        {
            var result = new List<PowerPlanInfo>();
            string output = Run("powercfg", "/list");

            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"([A-Fa-f0-9\-]{36}).*\((.*?)\)");
                if (match.Success)
                {
                    result.Add(new PowerPlanInfo
                    {
                        Guid = match.Groups[1].Value.Trim(),
                        Name = match.Groups[2].Value.Trim()
                    });
                }
            }

            return result;
        }

        public string GetActivePlanGuid()
        {
            string output = Run("powercfg", "/getactivescheme");
            var match = Regex.Match(output, @"([A-Fa-f0-9\-]{36})");
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        public string GetActivePlanName()
        {
            string output = Run("powercfg", "/getactivescheme");
            var match = Regex.Match(output, @"\((.*?)\)");
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        public void SetActivePlan(string planGuid)
        {
            Run("powercfg", $"/setactive {planGuid}");
        }

        public string DuplicateActivePlan(string newName)
        {
            string activeGuid = GetActivePlanGuid();
            string output = Run("powercfg", $"/duplicatescheme {activeGuid}");
            var match = Regex.Match(output, @"([A-Fa-f0-9\-]{36})");

            if (!match.Success)
                return "";

            string newGuid = match.Groups[1].Value.Trim();
            Run("powercfg", $"/changename {newGuid} \"{newName}\"");
            return newGuid;
        }

        public void ApplyProcessorSettings(
            string planGuid,
            int acParkingPercent,
            int dcParkingPercent,
            int acMaxFreqPercent,
            int dcMaxFreqPercent,
            bool acParkingEnabled,
            bool dcParkingEnabled,
            bool acFreqScalingEnabled,
            bool dcFreqScalingEnabled)
        {
            int acParkingValue = acParkingEnabled ? Clamp(acParkingPercent, 0, 100) : 100;
            int dcParkingValue = dcParkingEnabled ? Clamp(dcParkingPercent, 0, 100) : 100;

            int acFreqValue = acFreqScalingEnabled ? Clamp(acMaxFreqPercent, 1, 100) : 100;
            int dcFreqValue = dcFreqScalingEnabled ? Clamp(dcMaxFreqPercent, 1, 100) : 100;

            Run("powercfg", $"/setacvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_CORE_PARKING_MIN_CORES} {acParkingValue}");
            Run("powercfg", $"/setdcvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_CORE_PARKING_MIN_CORES} {dcParkingValue}");

            Run("powercfg", $"/setacvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_MAX_PROC_STATE} {acFreqValue}");
            Run("powercfg", $"/setdcvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_MAX_PROC_STATE} {dcFreqValue}");

            Run("powercfg", $"/setacvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_MIN_PROC_STATE} 5");
            Run("powercfg", $"/setdcvalueindex {planGuid} {SUB_PROCESSOR} {SETTING_MIN_PROC_STATE} 5");

            Run("powercfg", $"/setactive {planGuid}");
        }

        public string Run(string fileName, string arguments)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return string.IsNullOrWhiteSpace(output) ? error : output;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public PowerPlanInfo? FindByGuid(List<PowerPlanInfo> plans, string guid)
        {
            return plans.FirstOrDefault(x => x.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}