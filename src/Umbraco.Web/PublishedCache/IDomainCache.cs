using System.Collections.Generic;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache
{
    public interface IDomainCache
    {
        IEnumerable<Domain> GetAll(bool includeWildcards);
        IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards);
    }
}
