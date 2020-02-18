using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.Navision
{
    public class AccumulatedModel
    {
        public string Subscriber { get; set; }
        public string ZoneName { get; set; }
        public int styk { get; set; }
        public int Seconds { get; set; }
        public string Call_No { get; set; }
        public decimal Call_price { get; set; }
        public string Minute_No { get; set; }
        public decimal Minute_price { get; set; }        
        public decimal Total { get; set; }
    }
}