using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.ModelsBuilder.Umbraco
{
    [ComposeBefore(typeof(NuCacheComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ModelsBuilderComposer : ComponentComposer<ModelsBuilderComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<UmbracoServices>(Lifetime.Singleton);
            composition.Configs.Add(() => new Config());

            if (composition.Configs.ModelsBuilder().ModelsMode == ModelsMode.PureLive)
                ComposeForLiveModels(composition);
            else if (composition.Configs.ModelsBuilder().EnableFactory)
                ComposeForDefaultModelsFactory(composition);
        }

        private void ComposeForDefaultModelsFactory(Composition composition)
        {
            composition.RegisterUnique<IPublishedModelFactory>(factory =>
            {
                var typeLoader = factory.GetInstance<TypeLoader>();
                var types = typeLoader
                    .GetTypes<PublishedElementModel>() // element models
                    .Concat(typeLoader.GetTypes<PublishedContentModel>()); // content models
                return new PublishedModelFactory(types);
            });
        }

        private void ComposeForLiveModels(Composition composition)
        {
            composition.RegisterUnique<IPublishedModelFactory, PureLiveModelFactory>();

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
    }
}
