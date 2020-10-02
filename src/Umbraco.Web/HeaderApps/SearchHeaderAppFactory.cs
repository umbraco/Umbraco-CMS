using System.Collections.Generic;
using Umbraco.Core.Models.Header;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.HeaderApps
{
    public class SearchHeaderAppFactory : IHeaderAppFactory
    {
        private HeaderApp _searchHeaderApp;

        public HeaderApp GetHeaderAppFor(IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            return _searchHeaderApp ?? (_searchHeaderApp = new HeaderApp
            {
                Name = "Open backoffice search",
                Alias = "openBackofficeSearch",
                Weight = -200,
                Icon = "icon-search",
                Hotkey = "ctrl+space",
                Action = "searchClick()"
            });
        }
    }
}
