using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Events;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.DependencyInjection;
using Umbraco.Web.WebAssets;

// This is the insanity that allows you to customize the RazorProjectEngineBuilder
[assembly: ProvideRazorExtensionInitializer("ModelsBuilderPureLive", typeof(ModelsBuilderRazorProjectBuilderExtension))]

namespace Umbraco.ModelsBuilder.Embedded.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds umbraco's embedded model builder support
        /// </summary>
        public static IUmbracoBuilder AddModelsBuilder(this IUmbracoBuilder builder)
        {
            builder.AddRazorProjectEngine();
            builder.Services.AddSingleton<UmbracoServices>();
            // TODO: I feel like we could just do builder.AddNotificationHandler<ModelsBuilderNotificationHandler>() and it
            // would automatically just register for all implemented INotificationHandler{T}?
            builder.AddNotificationHandler<UmbracoApplicationStarting, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<ServerVariablesParsing, ModelsBuilderNotificationHandler>();
            builder.Services.AddUnique<ModelsGenerator>();
            builder.Services.AddUnique<LiveModelsProvider>();
            builder.Services.AddUnique<OutOfDateModelsStatus>();
            builder.Services.AddUnique<ModelsGenerationError>();

            builder.Services.AddUnique<PureLiveModelFactory>();
            builder.Services.AddUnique<IPublishedModelFactory>(factory =>
            {
                ModelsBuilderSettings config = factory.GetRequiredService<IOptions<ModelsBuilderSettings>>().Value;
                if (config.ModelsMode == ModelsMode.PureLive)
                {
                    return factory.GetRequiredService<PureLiveModelFactory>();

                    // the following would add @using statement in every view so user's don't
                    // have to do it - however, then noone understands where the @using statement
                    // comes from, and it cannot be avoided / removed --- DISABLED

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
                    TypeLoader typeLoader = factory.GetRequiredService<TypeLoader>();
                    IPublishedValueFallback publishedValueFallback = factory.GetRequiredService<IPublishedValueFallback>();
                    IEnumerable<Type> types = typeLoader
                        .GetTypes<PublishedElementModel>() // element models
                        .Concat(typeLoader.GetTypes<PublishedContentModel>()); // content models
                    return new PublishedModelFactory(types, publishedValueFallback);
                }

                return null;
            });

            return builder;
        }

        /// <summary>
        /// Can be called if using an external models builder to remove the embedded models builder controller features
        /// </summary>
        public static IUmbracoBuilder DisableModelsBuilderControllers(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<DisableModelsBuilderNotificationHandler>();
            return builder;
        }

        private static IUmbracoBuilder AddRazorProjectEngine(this IUmbracoBuilder builder)
        {
            // TODO: This is super nasty, but we can at least tinker with this for now
            // this pre-builds the container just so we can extract the default RazorProjectEngine
            // in order to extract out all features and re-add them to ours

            // Since we cannot construct the razor engine like netcore does:
            // https://github.com/dotnet/aspnetcore/blob/336e05577cd8bec2000ffcada926189199e4cef0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/DependencyInjection/RazorRuntimeCompilationMvcCoreBuilderExtensions.cs#L86
            // because many things are internal we need to resort to this which is to get the default RazorProjectEngine
            // that is nornally created and use that to create/wrap our custom one.

            ServiceProvider initialServices = builder.Services.BuildServiceProvider();
            RazorProjectEngine defaultRazorProjectEngine = initialServices.GetRequiredService<RazorProjectEngine>();

            // copy the current collection, we need to use this later to rebuild a container
            // to re-create the razor compiler provider
            var initialCollection = new ServiceCollection
            {
                builder.Services
            };

            builder.Services.AddSingleton<RazorProjectEngine>(
                s => new RefreshingRazorProjectEngine(defaultRazorProjectEngine, s, s.GetRequiredService<PureLiveModelFactory>()));

            builder.Services.AddSingleton<IViewCompilerProvider>(
                s => new RefreshingRuntimeViewCompilerProvider(
                        () =>
                        {
                            // re-create the original container so that a brand new IViewCompilerProvider
                            // is produced, if we don't re-create the container then it will just return the same instance.
                            ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
                            return recreatedServices.GetRequiredService<IViewCompilerProvider>();
                        }, s.GetRequiredService<PureLiveModelFactory>()));

            builder.Services.AddSingleton<IRazorPageActivator>(
                s => new RefreshingRazorPageActivator(
                        () =>
                        {
                            // re-create the original container so that a brand new IRazorPageActivator
                            // is produced, if we don't re-create the container then it will just return the same instance.
                            ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
                            return recreatedServices.GetRequiredService<IRazorPageActivator>();
                        }, s.GetRequiredService<PureLiveModelFactory>()));

            builder.Services.AddSingleton<IRazorViewEngine>(
                s => new RefreshingRazorViewEngine(
                        () =>
                        {
                            // re-create the original container so that a brand new IRazorPageActivator
                            // is produced, if we don't re-create the container then it will just return the same instance.
                            ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
                            return recreatedServices.GetRequiredService<IRazorViewEngine>();
                        }, s.GetRequiredService<PureLiveModelFactory>()));

            return builder;
        }
    }

    internal class ModelsBuilderRazorProjectBuilderExtension : RazorExtensionInitializer
    {
        public override void Initialize(RazorProjectEngineBuilder builder)
        {
            // Finally, after jumping through many hoops, we can customize the builder.

            // Get our extension that launched this so we can access services
            ModelsBuilderAssemblyExtension mbExt = builder.Configuration.Extensions.OfType<ModelsBuilderAssemblyExtension>().First();

            // Now... customize

            // TODO: BUT This is called before all of the default options are done, argh! so you can't replace anything here anyways
        }
    }

    // We need a custom assembly extension so we can pass state into the initializer :/
    internal class ModelsBuilderAssemblyExtension : AssemblyExtension
    {
        public ModelsBuilderAssemblyExtension(IServiceProvider serviceProvider, string extensionName, Assembly assembly)
            : base(extensionName, assembly) => ServiceProvider = serviceProvider;

        public IServiceProvider ServiceProvider { get; }
    }

    // The default razor page activator keeps an internal cache of activations, this allows clearning that cache
    // TODO: Find out if we really need to clear this cache or not? Or if just clearing the view engine cache is enough?
    internal class RefreshingRazorPageActivator : IRazorPageActivator
    {
        private readonly Func<IRazorPageActivator> _defaultRazorPageActivatorFactory;
        private readonly PureLiveModelFactory _pureLiveModelFactory;
        private IRazorPageActivator _current;

        public RefreshingRazorPageActivator(
            Func<IRazorPageActivator> defaultRazorPageActivatorFactory,
            PureLiveModelFactory pureLiveModelFactory)
        {
            _pureLiveModelFactory = pureLiveModelFactory;
            _defaultRazorPageActivatorFactory = defaultRazorPageActivatorFactory;
            _current = _defaultRazorPageActivatorFactory();
            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        // TODO: Do we need to lock?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = _defaultRazorPageActivatorFactory();

        public void Activate(IRazorPage page, ViewContext context) => _current.Activate(page, context);
    }

    // We need to have a refreshing razor view engine - the default keeps an in memory cache of views and it cannot be cleared because
    // the cache key instance is internal and would require manually tracking all keys since it cannot be iterated.
    // So like other 'Refreshing' intances, we just create a brand new one and let the old one die therefore clearing the cache.
    internal class RefreshingRazorViewEngine : IRazorViewEngine
    {
        private IRazorViewEngine _current;
        private readonly PureLiveModelFactory _pureLiveModelFactory;
        private readonly Func<IRazorViewEngine> _defaultRazorViewEngineFactory;

        public RefreshingRazorViewEngine(Func<IRazorViewEngine> defaultRazorViewEngineFactory, PureLiveModelFactory pureLiveModelFactory)
        {
            _pureLiveModelFactory = pureLiveModelFactory;
            _defaultRazorViewEngineFactory = defaultRazorViewEngineFactory;
            _current = _defaultRazorViewEngineFactory();
            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        // TODO: Do we need to lock?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = _defaultRazorViewEngineFactory();

        public RazorPageResult FindPage(ActionContext context, string pageName) => _current.FindPage(context, pageName);

        public string GetAbsolutePath(string executingFilePath, string pagePath) => _current.GetAbsolutePath(executingFilePath, pagePath);

        public RazorPageResult GetPage(string executingFilePath, string pagePath) => _current.GetPage(executingFilePath, pagePath);

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => _current.FindView(context, viewName, isMainPage);

        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage) => _current.GetView(executingFilePath, viewPath, isMainPage);
    }

    // The default view compiler creates the compiler once and only once. That compiler will have a stale list of references
    // to build against so it needs to be re-created. The only way to do that due to internals is to wrap it and re-create
    // the default instance therefore resetting the compiler references. 
    // TODO: Find out if we really need to clear this cache or not? Or if just clearing the view engine cache is enough?
    internal class RefreshingRuntimeViewCompilerProvider : IViewCompilerProvider
    {
        private IViewCompilerProvider _current;
        private readonly Func<IViewCompilerProvider> _defaultViewCompilerProviderFactory;
        private readonly PureLiveModelFactory _pureLiveModelFactory;

        public RefreshingRuntimeViewCompilerProvider(
            Func<IViewCompilerProvider> defaultViewCompilerProviderFactory,
            PureLiveModelFactory pureLiveModelFactory)
        {
            _defaultViewCompilerProviderFactory = defaultViewCompilerProviderFactory;
            _pureLiveModelFactory = pureLiveModelFactory;
            _current = _defaultViewCompilerProviderFactory();
            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        // TODO: Do we need to lock?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = _defaultViewCompilerProviderFactory();

        public IViewCompiler GetCompiler() => _current.GetCompiler();
    }

    // TODO: Need to review this to see if this service is actually one we need to clear or not?
    // Does it hold cache? etc... I originally said "so that all of the underlying services are cleared"
    // but I'm not sure now if that's needed since we need the above which do hold caches.
    // The other problem is that because we are re-creating the above services from the default service collection,
    // the reference they will have for their RazorProjectEngine will not be this one, it will be the default one...
    // though I guess we can change that based on the ordering and resolving of the containers.
    // I'm just not entirely convinced we need this anymore?
    // ... Actually, it might be all related to this IMetadataReferenceFeature thing since that sort of needs to be refreshed.
    // guess we need to do some testing. But we need to ensure that there's not a bunch of different razor project engines being
    // referenced everywhere.
    internal class RefreshingRazorProjectEngine : RazorProjectEngine
    {
        private RazorProjectEngine _current;
        private readonly RazorProjectEngine _defaultRazorProjectEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly PureLiveModelFactory _pureLiveModelFactory;
        private readonly MethodInfo _createCodeDocumentCore;
        private readonly MethodInfo _createCodeDocumentDesignTimeCore;
        private readonly MethodInfo _processCore;

        public RefreshingRazorProjectEngine(
            RazorProjectEngine defaultRazorProjectEngine,
            IServiceProvider serviceProvider,
            PureLiveModelFactory pureLiveModelFactory)
        {
            _defaultRazorProjectEngine = defaultRazorProjectEngine;
            _serviceProvider = serviceProvider;
            _pureLiveModelFactory = pureLiveModelFactory;
            _current = CreateNew();
            Type engineType = _current.GetType();
            _createCodeDocumentCore = engineType.GetMethod(nameof(CreateCodeDocumentCore), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(RazorProjectItem) }, null);
            _createCodeDocumentDesignTimeCore = engineType.GetMethod(nameof(CreateCodeDocumentDesignTimeCore), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(RazorProjectItem) }, null);
            _processCore = engineType.GetMethod(nameof(ProcessCore), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(RazorCodeDocument) }, null);

            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        public override RazorConfiguration Configuration => _current.Configuration;

        public override RazorProjectFileSystem FileSystem => _current.FileSystem;

        public override RazorEngine Engine => _current.Engine;

        public override IReadOnlyList<IRazorProjectEngineFeature> ProjectFeatures => _current.ProjectFeatures;

        // TODO: Do we need to lock?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = CreateNew();

        private RazorConfiguration GetConfiguration()
        {
            RazorConfiguration defaultConfig = RazorConfiguration.Default;

            // TODO: It turns out you can add logic into the RazorProjectEngineBuilder by adding custom razor extensions to the configuration.
            var mbConfig = RazorConfiguration.Create(
                defaultConfig.LanguageVersion,
                "ModelsBuilderPureLiveConfig",
                new[] { new ModelsBuilderAssemblyExtension(_serviceProvider, "ModelsBuilderPureLive", typeof(RefreshingRazorProjectEngine).Assembly) });

            return mbConfig;
        }

        private RazorProjectEngine CreateNew()
        {
            // get the default
            RazorProjectFileSystem fileSystem = _serviceProvider.GetRequiredService<RazorProjectFileSystem>();

            // Create the project engine
            var projectEngine = RazorProjectEngine.Create(GetConfiguration(), fileSystem, builder =>
            {
                // TODO: All this hacking is because RazorExtensionInitializer doesn't give us access to the service provider

                // replace all features with the defaults
                builder.Features.Clear();

                foreach (IRazorEngineFeature f in _defaultRazorProjectEngine.EngineFeatures)
                {
                    builder.Features.Add(f);
                }

                foreach (IRazorProjectEngineFeature f in _defaultRazorProjectEngine.ProjectFeatures)
                {
                    builder.Features.Add(f);
                }

                // The razor engine only supports one instance of IMetadataReferenceFeature
                // so we need to jump through some hoops to allow multiple by using a wrapper.
                // so get the current ones, remove them from the list, create a wrapper of them and
                // our custom one and then add it back.
                var metadataReferenceFeatures = builder.Features.OfType<IMetadataReferenceFeature>().ToList();
                foreach (IMetadataReferenceFeature m in metadataReferenceFeatures)
                {
                    builder.Features.Remove(m);
                }

                // add our custom one to the list
                metadataReferenceFeatures.Add(new PureLiveMetadataReferenceFeature(_pureLiveModelFactory));

                // now add them to our wrapper and back into the features
                builder.Features.Add(new MetadataReferenceFeatureWrapper(metadataReferenceFeatures));
            });

            return projectEngine;
        }

        protected override RazorCodeDocument CreateCodeDocumentCore(RazorProjectItem projectItem)
            => (RazorCodeDocument)_createCodeDocumentCore.Invoke(_current, new[] { projectItem });

        protected override RazorCodeDocument CreateCodeDocumentDesignTimeCore(RazorProjectItem projectItem)
            => (RazorCodeDocument)_createCodeDocumentDesignTimeCore.Invoke(_current, new[] { projectItem });

        protected override void ProcessCore(RazorCodeDocument codeDocument)
            => _processCore.Invoke(_current, new[] { codeDocument });

    }

    /// <summary>
    /// Wraps multiple <see cref="IMetadataReferenceFeature"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is required because the razor engine only supports a single IMetadataReferenceFeature but their APIs don't state this,
    /// this is purely down to them doing this 'First' call: https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Razor/Microsoft.CodeAnalysis.Razor/src/CompilationTagHelperFeature.cs#L37
    /// So in order to have multiple, we need to have a wrapper.
    /// </para>
    /// </remarks>
    public class MetadataReferenceFeatureWrapper : IMetadataReferenceFeature
    {
        private readonly IReadOnlyList<IMetadataReferenceFeature> _metadataReferenceFeatures;
        private RazorEngine _engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataReferenceFeatureWrapper"/> class.
        /// </summary>
        public MetadataReferenceFeatureWrapper(IEnumerable<IMetadataReferenceFeature> metadataReferenceFeatures)
            => _metadataReferenceFeatures = metadataReferenceFeatures.ToList();

        /// <inheritdoc/>
        public IReadOnlyList<MetadataReference> References
            => _metadataReferenceFeatures.SelectMany(x => x.References).ToList();

        /// <inheritdoc/>
        public RazorEngine Engine
        {
            get => _engine;
            set
            {
                _engine = value;
                foreach (IMetadataReferenceFeature feature in _metadataReferenceFeatures)
                {
                    feature.Engine = value;
                }
            }
        }
    }

    /// <summary>
    /// A custom <see cref="IMetadataReferenceFeature"/> that will dynamically resolve a reference for razor based on the current PureLive assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default implementation of IMetadataReferenceFeature is https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/LazyMetadataReferenceFeature.cs
    /// which uses a ReferenceManager https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs
    /// to resolve it's references. This is done using ApplicationParts which would be nice and simple to use if we could, but the Razor engine ONLY works
    /// with application part assemblies that have physical files with physical paths. We don't want to load in our PureLive assemblies on physical paths because
    /// those files will be locked. Instead we load them in via bytes but this is not supported and we'll get an exception if we add them to application parts.
    /// The other problem with LazyMetadataReferenceFeature is that it doesn't support dynamic assemblies, it will just check what in application parts once and
    /// that's it which will not work for us in Pure Live.
    /// </para>
    /// </remarks>
    internal class PureLiveMetadataReferenceFeature : IMetadataReferenceFeature
    {
        // TODO: Even though I was hoping this would work and this does allow you to return a metadata reference dynamically at runtime, it doesn't make any
        // difference because the CSharpCompiler for razor only loads in it's references one time based on the initial reference checks:
        // https://github.com/dotnet/aspnetcore/blob/100ab02ea0214d49535fa56f33a77acd61fe039c/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CSharpCompiler.cs#L84
        // Since ReferenceManager resolves them once lazily and that's it. 

        private readonly PureLiveModelFactory _pureLiveModelFactory;

        public PureLiveMetadataReferenceFeature(PureLiveModelFactory pureLiveModelFactory) => _pureLiveModelFactory = pureLiveModelFactory;

        /// <inheritdoc/>
        public IReadOnlyList<MetadataReference> References
        {
            get
            {
                // TODO: This won't really work based on how the CSharp compiler works
                //if (_pureLiveModelFactory.CurrentModelsMetadataReference != null)
                //{
                //    //var reference = MetadataReference.CreateFromStream(null);
                //    //reference.
                //    return new[] { _pureLiveModelFactory.CurrentModelsMetadataReference };
                //}

                return Array.Empty<MetadataReference>();
            }
        }

        /// <inheritdoc/>
        public RazorEngine Engine { get; set; }
    }
}
