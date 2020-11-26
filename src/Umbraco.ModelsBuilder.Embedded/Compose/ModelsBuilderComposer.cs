using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;
using Umbraco.Core.Builder;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{
    [ComposeBefore(typeof(IPublishedCacheComposer))]
    public sealed class ModelsBuilderComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<ModelsBuilderComponent>();
            builder.Services.AddSingleton<UmbracoServices>();
            builder.Services.AddUnique<ModelsGenerator>();
            builder.Services.AddUnique<LiveModelsProvider>();
            builder.Services.AddUnique<OutOfDateModelsStatus>();
            builder.Services.AddUnique<ModelsGenerationError>();

            builder.Services.AddUnique<PureLiveModelFactory>();
            builder.Services.AddUnique<IPublishedModelFactory>(factory =>
            {
                var config = factory.GetRequiredService<IOptions<ModelsBuilderSettings>>().Value;
                if (config.ModelsMode == ModelsMode.PureLive)
                {
                    return factory.GetRequiredService<PureLiveModelFactory>();
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
                    var typeLoader = factory.GetRequiredService<TypeLoader>();
                    var publishedValueFallback = factory.GetRequiredService<IPublishedValueFallback>();
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
