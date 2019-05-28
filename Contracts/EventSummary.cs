using System;
using System.Collections.Generic;

namespace MSUSDemos.GraphCalendar {
    public class EventSummary {
        public int NumberOfEvents { get; set; }

        public double DurationOfEvents { get; set; }

        public bool MeetingWithSatya { get; set; }
        
        public bool MeetingWithJPC { get; set; }

        public Dictionary<string, int> EventGraph { get; set; }
    }
}