using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class IPVisionPriceFile
    {
        public string zone { get; set; }
        public decimal price_minute { get; set; }
        public decimal price_call { get; set; }
    }
}