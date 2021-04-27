using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.Features;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{


    [ComposeBefore(typeof(NuCacheComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ModelsBuilderComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Configs.Add<IModelsBuilderConfig>(() => new ModelsBuilderConfig());

            if (IsExternalModelsBuilderInstalled() == true)
            {
                ComposeForExternalModelsBuilder(composition);
                return;
            }

            composition.Components().Append<ModelsBuilderComponent>();
            composition.Register<UmbracoServices>(Lifetime.Singleton);

            composition.RegisterUnique<ModelsGenerator>();
            composition.RegisterUnique<LiveModelsProvider>();
            composition.RegisterUnique<OutOfDateModelsStatus>();
            composition.RegisterUnique<ModelsGenerationError>();

            if (composition.Configs.ModelsBuilder().ModelsMode == ModelsMode.PureLive)
                ComposeForLiveModels(composition);
            else if (composition.Configs.ModelsBuilder().EnableFactory)
                ComposeForDefaultModelsFactory(composition);
        }

        private static bool IsExternalModelsBuilderInstalled()
        {
            var assemblyNames = new[]
            {
                "Umbraco.ModelsBuilder",
                "ModelsBuilder.Umbraco"
            };

            try
            {
                foreach (var name in assemblyNames)
                {
                    var assembly = Assembly.Load(name);

                    if (assembly != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                //swallow exception, DLL must not be there
            }

            return false;
        }

        private void ComposeForExternalModelsBuilder(Composition composition)
        {
            composition.Logger.Info<ModelsBuilderComposer>("ModelsBuilder.Embedded is disabled, the external ModelsBuilder was detected.");
            composition.Components().Append<DisabledModelsBuilderComponent>();
            composition.Dashboards().Remove<ModelsBuilderDashboard>();
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
