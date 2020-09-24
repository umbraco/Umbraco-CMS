using System.Linq;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{
    [ComposeBefore(typeof(IPublishedCacheComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ModelsBuilderComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ModelsBuilderComponent>();
            composition.Register<UmbracoServices>(Lifetime.Singleton);
            composition.RegisterUnique<ModelsGenerator>();
            composition.RegisterUnique<LiveModelsProvider>();
            composition.RegisterUnique<OutOfDateModelsStatus>();
            composition.RegisterUnique<ModelsGenerationError>();

            composition.RegisterUnique<PureLiveModelFactory>();
            composition.RegisterUnique<IPublishedModelFactory>(factory =>
            {
                var config = factory.GetInstance<IOptions<ModelsBuilderSettings>>().Value;
                if (config.ModelsMode == ModelsMode.PureLive)
                {
                    return factory.GetInstance<PureLiveModelFactory>();
                    // the following would add @using statement in every view so user's don't
                    // have to do it - however, then noone understands where the @using statement
                    // comes from, and it cannot be avoided / removed --- DISABLED
                    //
                    /*
                    // no need for @using in views
                    // note:
                    //  we are NOT using the in-code attribute here, config is required
                    //  because that would require parsing the code... and what if it changes?
                    //  we can AddGlobalImport not sure we can remove one anyways
                    var modelsNamespace = Configuration.Config.ModelsNamespace;
                    if (string.IsNullOrWhiteSpace(modelsNamespace))
                        modelsNamespace = Configuration.Config.DefaultModelsNamespace;
                    System.Web.WebPages.Razor.WebPageRazorHost.AddGlobalImport(modelsNamespace);
                    */
                }
                else if (config.EnableFactory)
                {
                    var typeLoader = factory.GetInstance<TypeLoader>();
                    var publishedValueFallback = factory.GetInstance<IPublishedValueFallback>();
                    var types = typeLoader
                        .GetTypes<PublishedElementModel>() // element models
                        .Concat(typeLoader.GetTypes<PublishedContentModel>()); // content models
                    return new PublishedModelFactory(types, publishedValueFallback);
                }

                return null;
            });

        }
    }
}
