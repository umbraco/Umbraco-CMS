using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Logging;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppDefinitionCollection : BuilderCollectionBase<IContentAppDefinition>
    {
        private readonly ILogger _logger;

        public ContentAppDefinitionCollection(IEnumerable<IContentAppDefinition> items, ILogger logger)
            : base(items)
        {
            _logger = logger;
        }

        public IEnumerable<ContentApp> GetContentAppsFor(object o)
        {
            var apps = this.Select(x => x.GetContentAppFor(o)).WhereNotNull().OrderBy(x => x.Weight).ToList();

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
                _logger.Warn<ContentAppDefinitionCollection>($"Duplicate content app aliases found: {string.Join(",", dups)}");
            }

            return apps;
        }
    }
}
