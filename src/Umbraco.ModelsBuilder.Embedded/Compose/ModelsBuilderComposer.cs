using System;
using System.Collections;
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

//[assembly: PreApplicationStartMethod(typeof(ModelsBuilderComposer), "Initialize")]

namespace Umbraco.ModelsBuilder.Embedded.Compose
{

    //public class MyBuildProvider : RazorBuildProvider
    //{
    //    public override void GenerateCode(AssemblyBuilder assemblyBuilder)
    //    {
    //        base.GenerateCode(assemblyBuilder);
    //    }

    //    protected override WebPageRazorHost CreateHost()
    //    {
    //        return base.CreateHost();
    //    }

    //    public override CompilerType CodeCompilerType
    //    {
    //        get
    //        {
    //            var asdf =  base.CodeCompilerType;
    //            return asdf;
    //        }
    //    }
    //}

    [ComposeBefore(typeof(NuCacheComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ModelsBuilderComposer : ICoreComposer
    {
        //public static void Initialize()
        //{
        //    BuildProvider.RegisterBuildProvider(".cshtml", typeof(MyBuildProvider));
        //}

        public void Compose(Composition composition)
        {
            var isLegacyModelsBuilderInstalled = IsLegacyModelsBuilderInstalled();

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

        private static bool IsLegacyModelsBuilderInstalled()
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
        }

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
            // Bind to the CompilingPath event of the RazorBuildProvider. There are 3x events:
            // CompilingPath = occurs first
            // CodeGenerationCompleted = occurs second
            // CodeGenerationStarted = occurs last -- yes that is true
            // Removing the assembly in CodeGenerationStarted is too late since the ReferencedAssemblies have already been passed to it's underlying
            // AssemblyBuilder class which is used to generate the csc.exe command with all of the referenced assemblies so we will remove the embedded
            // assembly in CompilingPath. When in legacy mode, there's no code within the embedded assembly that should run, this effectively removes
            // this assembly from the app domain for razor views - this will *not* solve the ambiguous issue for other dynamically compiled code
            // such as code in App_Code. It would be possible to solve that issue with a custom build provider.
            RazorBuildProvider.CompilingPath += (sender, args) =>
            {
                if (!(sender is RazorBuildProvider provider)) return;

                var assemblySet = provider.GetType().GetProperty("ReferencedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(provider);
                var removeMethod = assemblySet.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                removeMethod.Invoke(assemblySet, new object[] { this.GetType().Assembly });
            };

            //RazorBuildProvider.CodeGenerationStarted += (sender, args) =>
            //{
            //    if (!(sender is RazorBuildProvider provider)) return;

            //    if (isLegacyModelsBuilderInstalled)
            //    {
            //        var assemblySet = provider.GetType().GetProperty("ReferencedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(provider);
            //        var removeMethod = assemblySet.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //        removeMethod.Invoke(assemblySet, new object[] { this.GetType().Assembly });
            //        //provider.AssemblyBuilder.AddAssemblyReference();

            //    }
            //};

            //RazorBuildProvider.CodeGenerationCompleted += (sender, args) =>
            //{
            //    if (!(sender is RazorBuildProvider provider)) return;

            //    if (isLegacyModelsBuilderInstalled)
            //    {
            //        var assemblies = provider.CodeCompilerType.CompilerParameters.ReferencedAssemblies;

            //        var assemblySet = provider.GetType().GetProperty("ReferencedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(provider);
            //        var removeMethod = assemblySet.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //        removeMethod.Invoke(assemblySet, new object[] { this.GetType().Assembly });
            //        //provider.AssemblyBuilder.AddAssemblyReference();

            //    }
            //};

            //AppDomain.CurrentDomain.TypeResolve += (sender, args) =>
            //{
            //    return null;
            //};

            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    return null;
            //};
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
}
