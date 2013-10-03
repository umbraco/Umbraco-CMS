using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface ITab
    {
        string Caption { get; }

        IEnumerable<IControl> Controls { get; }

        IAccess AccessRights { get; }
    }
}