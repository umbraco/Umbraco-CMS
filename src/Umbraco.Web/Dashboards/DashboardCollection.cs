using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    public class DashboardCollection : BuilderCollectionBase<IDashboardSection>
    {
        public DashboardCollection(IEnumerable<IDashboardSection> items)
            : base(items)
        { }
    }
}
