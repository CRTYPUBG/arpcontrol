using System;

namespace ARPControl
{
    public class EventLogItem
    {
        public DateTime Time { get; set; }
        public string Process { get; set; } = "";
        public string PowerProfile { get; set; } = "";
        public string Details { get; set; } = "";
    }
}