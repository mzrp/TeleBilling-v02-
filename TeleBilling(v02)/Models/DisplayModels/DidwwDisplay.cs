using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class DidwwDisplayInbound
    {
        public string DIDCounter { get; set; }
        public string DIDDate { get; set; }
        public string DIDSource { get; set; }
        public string DID { get; set; }
        public string DIDDestination { get; set; }
        public string DIDDuration { get; set; }
        public string DisconnectInitiator { get; set; }
        public string DisconnectCode { get; set; }
        public string Response { get; set; }
        public string TollFreeAmount { get; set; }
        public string TerminationAmount { get; set; }
        public string MeteredChannelsAmount { get; set; }
    }

    public class DidwwDisplayOutboundExtended
    {
        public List<DidwwDisplayOutbound> alldidwws { get; set; }
        public string pushresults { get; set; }
}

    // Time Start,Source,CLI,Destination,Duration,Billing Duration,Disconnect Code,Disconnect Reason,Rate,Charged,CDR Type,Country Name,Network Name,Trunk Name
    public class DidwwDisplayOutbound
    {
        public string Counter { get; set; }
        public string TimeStart { get; set; }
        public string Source { get; set; }
        public string CLI { get; set; }
        public string Destination { get; set; }
        public string Duration { get; set; }
        public string BillingDuration { get; set; }
        public string DisconnectCode { get; set; }
        public string DisconnectReason { get; set; }
        public string Rate { get; set; }
        public string Charged { get; set; }
        public string CDRType { get; set; }
        public string CountryName { get; set; }
        public string NetworkName { get; set; }
        public string TrunkName { get; set; }
        public string Prefix { get; set; }
        public string RackpeopleCharge { get; set; }
        public string DestinationNetwork { get; set; }
        public string FinalChargeK { get; set; }
        public string FinalChargeO { get; set; }
        public string MinutePrice { get; set; }
        public string SecondPrice { get; set; }
    }
}