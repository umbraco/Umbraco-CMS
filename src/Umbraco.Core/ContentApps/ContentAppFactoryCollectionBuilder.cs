using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<ContentAppFactoryCollectionBuilder, ContentAppFactoryCollection, IContentAppFactory>
{
    protected override ContentAppFactoryCollectionBuilder This => this;

    // need to inject dependencies in the collection, so override creation
    public override ContentAppFactoryCollection CreateCollection(IServiceProvider factory)
    {
        // get the logger factory just-in-time - see note below for manifest parser
        ILoggerFactory loggerFactory = factory.GetRequiredService<ILoggerFactory>();
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor =
            factory.GetRequiredService<IBackOfficeSecurityAccessor>();
        return new ContentAppFactoryCollection(() => CreateItems(factory), loggerFactory.CreateLogger<ContentAppFactoryCollection>(), backOfficeSecurityAccessor);
    }

    protected override IEnumerable<IContentAppFactory> CreateItems(IServiceProvider factory)
    {
        // get the manifest parser just-in-time - injecting it in the ctor would mean that
        // simply getting the builder in order to configure the collection, would require
        // its dependencies too, and that can create cycles or other oddities
        IManifestParser manifestParser = factory.GetRequiredService<IManifestParser>();
        IIOHelper ioHelper = factory.GetRequiredService<IIOHelper>();
        return base.CreateItems(factory)
            .Concat(manifestParser.CombinedManifest.ContentApps.Select(x =>
                new ManifestContentAppFactory(x, ioHelper)));
    }
}
