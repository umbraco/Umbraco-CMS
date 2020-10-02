using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models.Header
{
    public interface IHeaderAppFactory
    {
        HeaderApp GetHeaderAppFor(IEnumerable<IReadOnlyUserGroup> userGroups);
    }
}
