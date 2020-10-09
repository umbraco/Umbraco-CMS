using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache
{
    public struct RouteByIdResult
    {
        public RouteByIdResult(RoutingOutcome outcome, string url = null,int? domainId = null)
        {
            Outcome = outcome;
            Url = url;
            DomainId = domainId;
        }
        public string Url { get;}
        public int? DomainId { get; }
        public RoutingOutcome Outcome { get; }
    }
}
