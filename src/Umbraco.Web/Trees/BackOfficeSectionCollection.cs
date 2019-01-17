using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Trees
{
    public class BackOfficeSectionCollection : BuilderCollectionBase<IBackOfficeSection>
    {
        public BackOfficeSectionCollection(IEnumerable<IBackOfficeSection> items)
            : base(items)
        { }
    }

    public class BackOfficeSectionCollectionBuilder : LazyCollectionBuilderBase<BackOfficeSectionCollectionBuilder, BackOfficeSectionCollection, IBackOfficeSection>
    {
        protected override BackOfficeSectionCollectionBuilder This => this;

        //TODO: can we allow for re-ordering OOTB without exposing other methods?
    }
}
