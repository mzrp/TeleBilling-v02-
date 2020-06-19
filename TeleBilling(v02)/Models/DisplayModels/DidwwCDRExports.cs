using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class Filters
    {
        public string year { get; set; }
        public string month { get; set; }
    }

    public class AttributesC
    {
        public Filters filters { get; set; }
        public string status { get; set; }
        public DateTime? created_at { get; set; }
        public string url { get; set; }
    }

    public class DatumC
    {
        public string id { get; set; }
        public string type { get; set; }
        public AttributesC attributes { get; set; }
    }

    public class MetaC
    {
        public int? total_records { get; set; }
    }

    public class CDRExports
    {
        public List<DatumC> data { get; set; }
        public MetaC meta { get; set; }
    }

    public class FiltersR
    {
        public string year { get; set; }
        public string month { get; set; }
        public string did_number { get; set; }
    }

    public class AttributesR
    {
        public FiltersR filters { get; set; }
        public string status { get; set; }
        public DateTime? created_at { get; set; }
        public object url { get; set; }
    }

    public class DataR
    {
        public string id { get; set; }
        public string type { get; set; }
        public AttributesR attributes { get; set; }
    }

    public class CDRExportReportResponse
    {
        public DataR data { get; set; }
    }

    public class FiltersSE
    {
        public string year { get; set; }
        public string month { get; set; }
    }

    public class AttributesSE
    {
        public FiltersSE filters { get; set; }
        public string status { get; set; }
        public DateTime? created_at { get; set; }
        public string url { get; set; }
    }

    public class DataSE
    {
        public string id { get; set; }
        public string type { get; set; }
        public AttributesSE attributes { get; set; }
    }

    public class CDRSingleExport
    {
        public DataSE data { get; set; }
    }

}