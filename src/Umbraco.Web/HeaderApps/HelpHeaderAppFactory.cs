using System.Collections.Generic;
using Umbraco.Core.Models.Header;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.HeaderApps
{
    public class HelpHeaderAppFactory : IHeaderAppFactory
    {
        private HeaderApp _helpHeaderApp;

        public HeaderApp GetHeaderAppFor(IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            return _helpHeaderApp ?? (_helpHeaderApp = new HeaderApp
            {
                Name = "Open/Close backoffice help",
                Alias = "openCloseBackofficeHelp",
                Weight = -100,
                Icon = "icon-help-alt",
                Hotkey = "ctrl+shift+h",
                View = "views/header/apps/help/help.html"
            });
        }
    }
}
