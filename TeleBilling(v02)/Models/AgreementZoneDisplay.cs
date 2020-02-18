using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models
{
    public class AgreementZoneDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Call_price { get; set; }
        public decimal Minute_price { get; set; }
        public decimal Call_price_RP { get; set; }
        public decimal Minute_price_RP { get; set; }
        public string Country_code { get; set; }
        public Nullable<int> CSVFileId { get; set; }
        public Nullable<int> AgreementId { get; set; }

        public virtual CSVFile CSVFile { get; set; }
        public virtual Agreement Agreement { get; set; }
    }
}