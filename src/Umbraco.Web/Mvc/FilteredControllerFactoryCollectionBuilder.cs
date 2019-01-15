using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class FilteredControllerFactoryCollectionBuilder : OrderedCollectionBuilderBase<FilteredControllerFactoryCollectionBuilder, FilteredControllerFactoryCollection, IFilteredControllerFactory>
    {
        protected override FilteredControllerFactoryCollectionBuilder This => this;
    }
}
