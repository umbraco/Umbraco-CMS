using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public class BackOfficeSectionCollection : BuilderCollectionBase<IBackOfficeSection>
    {
        public BackOfficeSectionCollection(IEnumerable<IBackOfficeSection> items)
            : base(items)
        { }
    }
}
