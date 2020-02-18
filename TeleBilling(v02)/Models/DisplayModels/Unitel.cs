using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models
{
    public class Unitel
    {
        public string CustomerId { get; set; }
        public string Trunk { get; set; }
        public string CallId { get; set; }
        public string From { get; set; }
        public string To{ get; set; }
        public string StartTime { get; set; }
        public string Type { get; set; }
        public string Duration { get; set; }
        public string Price{ get; set; }
        public string BillingZoneId { get; set; }
        public string BillingZonePrefix { get; set; }
        public string BillingZoneDescription { get; set; }
        public string BillingZoneCallFee { get; set; }
        public string BillingZoneCallRate { get; set; }

    }
}