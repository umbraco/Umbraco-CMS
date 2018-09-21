using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppDefinitionCollection : BuilderCollectionBase<IContentAppDefinition>
    {
        public ContentAppDefinitionCollection(IEnumerable<IContentAppDefinition> items)
            : base(items)
        { }

        public IEnumerable<ContentApp> GetContentAppsFor(object o)
        {
            return this.Select(x => x.GetContentAppFor(o)).WhereNotNull().OrderBy(x => x.Weight);
        }
    }
}
