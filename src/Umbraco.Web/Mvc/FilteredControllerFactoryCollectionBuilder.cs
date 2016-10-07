using LightInject;
using Umbraco.Core.DI;

namespace Umbraco.Web.Mvc
{
    public class FilteredControllerFactoryCollectionBuilder : OrderedCollectionBuilderBase<FilteredControllerFactoryCollectionBuilder, FilteredControllerFactoryCollection, IFilteredControllerFactory>
    {
        public FilteredControllerFactoryCollectionBuilder(IServiceContainer container) 
            : base(container)
        { }

        protected override FilteredControllerFactoryCollectionBuilder This => this;
    }
}
