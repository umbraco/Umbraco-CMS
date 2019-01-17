using Umbraco.Core.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public class BackOfficeSectionCollectionBuilder : OrderedCollectionBuilderBase<BackOfficeSectionCollectionBuilder, BackOfficeSectionCollection, IBackOfficeSection>
    {
        protected override BackOfficeSectionCollectionBuilder This => this;
    }
}
