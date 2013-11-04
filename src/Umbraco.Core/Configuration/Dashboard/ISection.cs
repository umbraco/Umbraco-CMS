using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface ISection
    {
        string Alias { get; }

        IEnumerable<string> Areas { get; }

        IEnumerable<IDashboardTab> Tabs { get; }

        IAccess AccessRights { get; }
    }
}