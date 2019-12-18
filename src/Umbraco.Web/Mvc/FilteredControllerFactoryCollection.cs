using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class FilteredControllerFactoryCollection : BuilderCollectionBase<IFilteredControllerFactory>
    {
        public FilteredControllerFactoryCollection(IEnumerable<IFilteredControllerFactory> items)
            : base(items)
        { }
    }
}
