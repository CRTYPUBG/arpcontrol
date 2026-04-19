using System;
using System.Collections.Generic;
using System.Linq;

namespace ARPControl
{
    public static class EventLogger
    {
        private static readonly List<EventLogItem> _items = new();

        public static void Add(string process, string profile, string details = "")
        {
            _items.Insert(0, new EventLogItem
            {
                Time = DateTime.Now,
                Process = process,
                PowerProfile = profile,
                Details = details
            });

            if (_items.Count > 500)
                _items.RemoveAt(_items.Count - 1);
        }

        public static List<EventLogItem> GetAll()
        {
            return _items.ToList();
        }
    }
}