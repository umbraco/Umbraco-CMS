using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface ISection
    {
        string Alias { get; }

        string Area { get; }

        IEnumerable<ITab> Tabs { get; }

        IAccess AccessRights { get; }
    }
}