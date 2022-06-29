using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

public class DashboardCollection : BuilderCollectionBase<IDashboard>
{
    public DashboardCollection(Func<IEnumerable<IDashboard>> items)
        : base(items)
    {
    }
}
