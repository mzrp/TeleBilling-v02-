//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TeleBilling_v02_.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ZoneRecords
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Call_price { get; set; }
        public decimal Minute_price { get; set; }
        public string Country_code { get; set; }
        public Nullable<int> CSVFileId { get; set; }
        public Nullable<int> AgreementId { get; set; }
    
        public virtual CSVFile CSVFile { get; set; }
        public virtual Agreement Agreement { get; set; }
    }
}
