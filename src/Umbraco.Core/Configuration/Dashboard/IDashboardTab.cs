using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IDashboardTab
    {
        string Caption { get; }

        IEnumerable<IDashboardControl> Controls { get; }

        IAccess AccessRights { get; }
    }
}