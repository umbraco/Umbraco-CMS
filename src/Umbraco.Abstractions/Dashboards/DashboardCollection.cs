using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    public class DashboardCollection : BuilderCollectionBase<IDashboard>
    {
        public DashboardCollection(IEnumerable<IDashboard> items)
            : base(items)
        { }
    }
}
