using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleBilling_v02_.Models.DisplayModels
{
    public class Attributes
    {
        public bool? blocked { get; set; }
        public int? capacity_limit { get; set; }
        public string description { get; set; }
        public bool? terminated { get; set; }
        public bool? awaiting_registration { get; set; }
        public string number { get; set; }
        public DateTime? expires_at { get; set; }
        public int? channels_included_count { get; set; }
        public DateTime? created_at { get; set; }
        public bool? pending_removal { get; set; }
        public int? dedicated_channels_count { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class DidGroup
    {
        public Links links { get; set; }
    }

    public class Links2
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class Order
    {
        public Links2 links { get; set; }
    }

    public class Links3
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class Trunk
    {
        public Links3 links { get; set; }
    }

    public class Links4
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class TrunkGroup
    {
        public Links4 links { get; set; }
    }

    public class Links5
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class CapacityPool
    {
        public Links5 links { get; set; }
    }

    public class Links6
    {
        public string self { get; set; }
        public string related { get; set; }
    }

    public class SharedCapacityGroup
    {
        public Links6 links { get; set; }
    }

    public class Relationships
    {
        public DidGroup did_group { get; set; }
        public Order order { get; set; }
        public Trunk trunk { get; set; }
        public TrunkGroup trunk_group { get; set; }
        public CapacityPool capacity_pool { get; set; }
        public SharedCapacityGroup shared_capacity_group { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public string type { get; set; }
        public Attributes attributes { get; set; }
        public Relationships relationships { get; set; }
    }

    public class Meta
    {
        public int total_records { get; set; }
    }

    public class Links7
    {
        public string first { get; set; }
        public string last { get; set; }
    }

    public class Dids
    {
        public List<Datum> data { get; set; }
        public Meta meta { get; set; }
        public Links7 links { get; set; }
    }
}