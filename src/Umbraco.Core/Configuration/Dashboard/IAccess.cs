using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IAccess
    {
        IEnumerable<IAccessItem> Rules { get; }
    }
}