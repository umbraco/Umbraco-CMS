using Umbraco.Core.Composing;
using Umbraco.Core.Models.Header;
using Umbraco.Core.Logging;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using System.Linq;
using Umbraco.Core;

namespace Umbraco.Web.HeaderApps
{
    public class HeaderAppFactoryCollection : BuilderCollectionBase<IHeaderAppFactory>
    {
        private readonly ILogger _logger;

        public HeaderAppFactoryCollection(IEnumerable<IHeaderAppFactory> items, ILogger logger) : base (items)
        {
            _logger = logger;
        }

        private IEnumerable<IReadOnlyUserGroup> GetCurrentUserGroups()
        {
            var umbracoContext = Composing.Current.UmbracoContext;
            var currentUser = umbracoContext?.Security?.CurrentUser;
            return currentUser is null
                ? Enumerable.Empty<IReadOnlyUserGroup>()
                : currentUser.Groups;
        }

        public IEnumerable<HeaderApp> GetHeaderAppsFor()
        {
            var roles = GetCurrentUserGroups();

            var apps = this.Select(x => x.GetHeaderAppFor(roles)).WhereNotNull().OrderBy(x => x.Weight).ToList();

            var aliasses = new HashSet<string>();
            List<string> dups = null;

            foreach (var app in apps)
            {
                if (aliasses.Contains(app.Alias))
                    (dups ?? (dups = new List<string>())).Add(app.Alias);
                else
                    aliasses.Add(app.Alias);
            }

            if (dups != null)
            {
                _logger.Warn<HeaderAppFactoryCollection>("Duplicate content app aliases found: {DuplicateAliases}", string.Join(",", dups));
            }

            return apps;
        }
    }
}
