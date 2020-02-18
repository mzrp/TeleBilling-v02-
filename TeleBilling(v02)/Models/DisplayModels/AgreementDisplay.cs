using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class AgreementDisplay
    {
        public string Customer_cvr { get; set; }
        public string Customer_name { get; set; }
        public string Subscriber_range_start { get; set; }
        public string Subscriber_range_end { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public DateTime Date { get; set; }

        public List<AgreementZoneRecords> ZoneRecords { get; set; }
    }
}