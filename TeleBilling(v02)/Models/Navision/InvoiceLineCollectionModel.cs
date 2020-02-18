using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.Navision
{
    public class InvoiceLineCollectionModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Subscriber_Range_Start { get; set; }
        public string Subscriber_Range_End { get; set; }
        public string Agreement_Description { get; set; }
        public List<ZoneLinesModel> ZoneLines { get; set; }
        public List<AccumulatedModel> Accumulated { get; set; }
    }
}