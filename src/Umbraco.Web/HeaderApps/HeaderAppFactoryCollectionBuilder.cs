using Umbraco.Core.Composing;
using Umbraco.Core.Models.Header;

namespace Umbraco.Web.HeaderApps
{
    public class HeaderAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<HeaderAppFactoryCollectionBuilder, HeaderAppFactoryCollection, IHeaderAppFactory>
    {
        protected override HeaderAppFactoryCollectionBuilder This => this;
    }
}
