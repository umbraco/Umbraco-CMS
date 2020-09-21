using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<ContentAppFactoryCollectionBuilder, ContentAppFactoryCollection, IContentAppFactory>
    {
        protected override ContentAppFactoryCollectionBuilder This => this;

        // need to inject dependencies in the collection, so override creation
        public override ContentAppFactoryCollection CreateCollection(IFactory factory)
        {
            // get the logger factory just-in-time - see note below for manifest parser
            var loggerFactory = factory.GetInstance<ILoggerFactory>();
            var umbracoContextAccessor = factory.GetInstance<IUmbracoContextAccessor>();
            return new ContentAppFactoryCollection(CreateItems(factory), loggerFactory.CreateLogger<ContentAppFactoryCollection>(), umbracoContextAccessor);
        }

        protected override IEnumerable<IContentAppFactory> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<IManifestParser>();
            var ioHelper = factory.GetInstance<IIOHelper>();
            return base.CreateItems(factory).Concat(manifestParser.Manifest.ContentApps.Select(x => new ManifestContentAppFactory(x, ioHelper)));
        }
    }
}
