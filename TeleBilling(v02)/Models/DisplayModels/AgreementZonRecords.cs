using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class AgreementZoneRecords
    {
        public int Id { get; set; }
        public string Country_code { get; set; }
        public string Name { get; set; }
        public decimal Call_price_Supplier { get; set; }
        public decimal Minute_price_Supplier { get; set; }
        public decimal Call_price_RP { get; set; }
        public decimal Minute_price_RP { get; set; }
        
    }
}