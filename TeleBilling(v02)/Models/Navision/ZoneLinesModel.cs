using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models
{
    public class ZoneLinesModel
    {
        public string ZoneName { get; set; }
        public int ZoneCalls { get; set; }
        public string ZoneCallNo { get; set; }
        public int ZoneSeconds { get; set; }
        public string ZoneMinuteNo { get; set; }
        public decimal ZonePriceMinute { get; set; }
        public decimal ZonePriceCall { get; set; }
    }
}