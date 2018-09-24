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
        //private readonly ILogger _logger;

        public ContentAppDefinitionCollection(IEnumerable<IContentAppDefinition> items)
            : base(items)
        {
            //_logger = logger;
        }

        public IEnumerable<ContentApp> GetContentAppsFor(object o)
        {
            var apps = this.Select(x => x.GetContentAppFor(o)).WhereNotNull().OrderBy(x => x.Weight);
            //apps must have unique aliases, we will remove any duplicates and log problems
            var resultApps = new Dictionary<string, ContentApp>();
            List<string> dups = null;
            foreach(var a in apps)
            {
                if (resultApps.TryGetValue(a.Alias, out var count))
                    (dups ?? (dups = new List<string>())).Add(a.Alias);
                else
                    resultApps[a.Alias] = a;
            }
            if (dups != null)
            {
                throw new InvalidOperationException($"Duplicate content app aliases found: {string.Join(",", dups)}");
                //_logger.Warn<ContentAppDefinitionCollection>($"Duplicate content app aliases found: {string.Join(",", dups)}");
            }

            return resultApps.Values;
        }
    }
}
