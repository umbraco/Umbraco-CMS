using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages.Razor;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.Compose;
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
            var isLegacyModelsBuilderInstalled = IsLegacyModelsBuilderInstalled.Value;

            composition.Configs.Add<IModelsBuilderConfig>(() => new ModelsBuilderConfig());

            if (isLegacyModelsBuilderInstalled)
            {
                ComposeForLegacyModelsBuilder(composition);
                ConfigureRazorBuildProviderForLegacyModelsBuilder();
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

        internal static Lazy<bool> IsLegacyModelsBuilderInstalled { get; } = new Lazy<bool>(() =>
        {
            Assembly legacyMbAssembly = null;
            try
            {
                legacyMbAssembly = Assembly.Load("Umbraco.ModelsBuilder");
            }
            catch (System.Exception)
            {
                //swallow exception, DLL must not be there
            }

            return legacyMbAssembly != null;
        });

        /// <summary>
        /// Adds custom event handling to the RazorBuildProvider to deal with ambiguous method calls
        /// </summary>
        /// <remarks>
        /// Because the embedded models builder and the legacy models builder share the same namespaces and method names
        /// we can get ambiguous method name exceptions.
        /// When in legacy mode will just ensure that the embedded assembly is not added to the references assemblies collection
        /// during razor view compilation.
        /// Unfortunately this process isn't that pretty because the APIs they expose aren't very flexible. 
        /// </remarks>
        private void ConfigureRazorBuildProviderForLegacyModelsBuilder()
        {
            // Bind to this event to remove the embedded assembly from the underlying AssemblyBuilder. This is the latest we can remove
            // this assembly and it will only be removed from the collection attached to this particular AssemblyBuilder. It is possible
            // to modify the RazorBuildProvider.ReferencedAssemblies collection in the same way, however that collection is a shared collection
            // between all build providers so this is sort of safer.
            RazorBuildProvider.CodeGenerationStarted += (sender, args) =>
            {
                if (!(sender is RazorBuildProvider provider)) return;

                // Remove the embedded assembly from the assembly builder
                provider.AssemblyBuilder.RemoveAssemblyReference(typeof(ModelsBuilderComposer).Assembly);
            };
        }

        private void ComposeForLegacyModelsBuilder(Composition composition)
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

    internal static class RazorBuildProviderExtensions
    {
        public static void RemoveAssemblyReference(this AssemblyBuilder assemblyBuilder, Assembly assembly)
        {
            if (assemblyBuilder == null) throw new ArgumentNullException(nameof(assemblyBuilder));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var assemblySet = RefAssemblies.Value?.GetValue(assemblyBuilder);

            // this is not thread safe but that's ok, it would just get overwritten
            if (_removeMethod == null)
            {
                _removeMethod = assemblySet?.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (_removeMethod == null)
                    throw new InvalidOperationException("Could not reflect required Remove property");
            }
            _removeMethod.Invoke(assemblySet, new object[] { assembly });
        }

        private static readonly Lazy<FieldInfo> RefAssemblies = new Lazy<FieldInfo>(() => typeof(AssemblyBuilder).GetField("_initialReferencedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic));
        private static MethodInfo _removeMethod;
    }
}
