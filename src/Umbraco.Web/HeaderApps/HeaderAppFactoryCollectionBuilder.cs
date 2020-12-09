using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Header;

namespace Umbraco.Web.HeaderApps
{
    public class HeaderAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<HeaderAppFactoryCollectionBuilder, HeaderAppFactoryCollection, IHeaderAppFactory>
    {
        protected override HeaderAppFactoryCollectionBuilder This => this;

        public override HeaderAppFactoryCollection CreateCollection(IFactory factory)
        {
            var logger = factory.GetInstance<ILogger>();

            return new HeaderAppFactoryCollection(CreateItems(factory), logger);
        }

        protected override IEnumerable<IHeaderAppFactory> CreateItems(IFactory factory)
        {
            var manifestParser = factory.GetInstance<ManifestParser>();

            return base.CreateItems(factory).Concat(manifestParser.Manifest.HeaderApps.Select(x => new ManifestHeaderAppFactory(x)));
        }
    }
}
