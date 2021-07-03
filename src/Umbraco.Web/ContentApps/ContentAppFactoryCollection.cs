using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppFactoryCollection : BuilderCollectionBase<IContentAppFactory>
    {
        private readonly ILogger _logger;

        public ContentAppFactoryCollection(IEnumerable<IContentAppFactory> items, ILogger logger)
            : base(items)
        {
            _logger = logger;
        }

        private IEnumerable<IReadOnlyUserGroup> GetCurrentUserGroups()
        {
            var umbracoContext = Composing.Current.UmbracoContext;
            var currentUser = umbracoContext?.Security?.CurrentUser;
            return currentUser == null
                ? Enumerable.Empty<IReadOnlyUserGroup>()
                : currentUser.Groups;

        }

        public IEnumerable<ContentApp> GetContentAppsFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups=null)
        {
            var roles = GetCurrentUserGroups();

            var apps = this.Select(x => x.GetContentAppFor(o, roles)).WhereNotNull().OrderBy(x => x.Weight).ToList();

            var aliases = new HashSet<string>();
            List<string> dups = null;

            foreach (var app in apps)
            {
                if (aliases.Contains(app.Alias))
                    (dups ?? (dups = new List<string>())).Add(app.Alias);
                else
                    aliases.Add(app.Alias);
            }

            if (dups != null)
            {
                // dying is not user-friendly, so let's write to log instead, and wish people read logs...

                //throw new InvalidOperationException($"Duplicate content app aliases found: {string.Join(",", dups)}");
                _logger.Warn<ContentAppFactoryCollection, string>("Duplicate content app aliases found: {DuplicateAliases}", string.Join(",", dups));
            }

            return apps;
        }
    }
}
