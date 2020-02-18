using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.Navision
{
    public class InvoiceModel
    {
        public string CVR { get; set; }
        public List<InvoiceLineCollectionModel> LineCollections { get; set; }
    }
}