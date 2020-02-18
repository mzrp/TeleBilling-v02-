using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models
{
    public class IPVision
    {
        public string id { get; set; }
        public string time { get; set; }
        public string subscriber { get; set; }
        public string aprefix { get; set; }
        public string destination { get; set; }
        public string invoicegroup { get; set; }
        public string prefix { get; set; }
        public string pbx { get; set; }
        public string direction { get; set; }
        public string volumetimesecs { get; set; }
        public string price { get; set; }
        public string free { get; set; }
        public string forward { get; set; }
        public string servingnetwork { get; set; }
        public string reason { get; set; }
        public string billed { get; set; }
        public string zone { get; set; }
    }
        
}