using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards
{
    public class DashboardCollection : BuilderCollectionBase<IDashboard>
    {
        public DashboardCollection(IEnumerable<IDashboard> items)
            : base(items)
        { }
    }
}
