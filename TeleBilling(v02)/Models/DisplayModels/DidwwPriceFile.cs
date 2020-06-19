using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models
{
    public class DidwwPriceFile
    {
        public string Country { get; set; }
        public decimal International_Final_Sales { get; set; }
        public decimal Origin_Final_Sales { get; set; }
        public decimal Local_Final_Sales { get; set; }
        public string Billing_How { get; set; }
        public string Country_Code { get; set; }
    }
}