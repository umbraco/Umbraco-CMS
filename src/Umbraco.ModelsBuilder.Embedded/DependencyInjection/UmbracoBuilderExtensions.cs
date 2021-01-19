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

/*
 * OVERVIEW:
 * 
 * The CSharpCompiler is responsible for the actual compilation of razor at runtime.
 * It creates a CSharpCompilation instance to do the compilation. This is where DLL references
 * are applied. However, the way this works is not flexible for dynamic assemblies since the references
 * are only discovered and loaded once before the first compilation occurs. This is done here:
 * https://github.com/dotnet/aspnetcore/blob/114f0f6d1ef1d777fb93d90c87ac506027c55ea0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CSharpCompiler.cs#L79
 * The CSharpCompiler is internal and cannot be replaced or extended, however it's references come from:
 * RazorReferenceManager. Unfortunately this is also internal and cannot be replaced, though it can be extended
 * using MvcRazorRuntimeCompilationOptions, except this is the place where references are only loaded once which
 * is done with a LazyInitializer. See https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs#L35.
 * 
 * The way that RazorReferenceManager works is by resolving references from the ApplicationPartsManager - either by
 * an application part that is specifically an ICompilationReferencesProvider or an AssemblyPart. So to fulfill this
 * requirement, we add the MB assembly to the assembly parts manager within the PureLiveModelFactory when the assembly
 * is (re)generated. But due to the above restrictions, when re-generating, this will have no effect since the references
 * have already been resolved with the LazyInitializer in the RazorReferenceManager.
 * 
 * The services that can be replaced are: IViewCompilerProvider (default is the internal RuntimeViewCompilerProvider) and
 * IViewCompiler (default is the internal RuntimeViewCompiler).
 * 
 * There are caches at several levels, all of which are not publicly accessible APIs (apart from RazorViewEngine.ViewLookupCache).
 * For this to work, several caches must be cleared:
 * - RazorViewEngine.ViewLookupCache
 * - RazorReferencesManager._compilationReferences
 * - RazorPageActivator._activationInfo (though this one may be optional)
 * - RuntimeViewCompiler._cache
 * 
 * What are our options?
 * 
 * a) We can copy a ton of code into our application: CSharpCompiler, RuntimeViewCompilerProvider, RuntimeViewCompiler and
 *    RazorReferenceManager (probably more depending on the extent of Internal references).
 * b) We can use reflection to try to access all of the above resources and try to forcefully clear caches and reset initialization flags.
 * c) We hack these replace-able services with our own implementations that wrap the default services. To do this
 *    requires re-resolving the original services from a pre-built DI container. In effect this re-creates these
 *    services from scratch which means there is no caches.
 *
 * ... Option C works, we will use that but need to verify how this affects memory since ideally the old services will be GC'd.
 */

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

            //builder.Services.AddSingleton<RazorProjectEngine>(
            //    s => new RefreshingRazorProjectEngine(defaultRazorProjectEngine, s, s.GetRequiredService<PureLiveModelFactory>()));

            //builder.Services.AddSingleton<IViewCompilerProvider>(
            //    s => new RefreshingRuntimeViewCompilerProvider(
            //            () =>
            //            {
            //                // re-create the original container so that a brand new IViewCompilerProvider
            //                // is produced, if we don't re-create the container then it will just return the same instance.
            //                ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
            //                return recreatedServices.GetRequiredService<IViewCompilerProvider>();
            //            }, s.GetRequiredService<PureLiveModelFactory>()));

            //builder.Services.AddSingleton<IRazorPageActivator>(
            //    s => new RefreshingRazorPageActivator(
            //            () =>
            //            {
            //                // re-create the original container so that a brand new IRazorPageActivator
            //                // is produced, if we don't re-create the container then it will just return the same instance.
            //                ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
            //                return recreatedServices.GetRequiredService<IRazorPageActivator>();
            //            }, s.GetRequiredService<PureLiveModelFactory>()));

            builder.Services.AddSingleton<IRazorViewEngine>(
                s => new RefreshingRazorViewEngine(
                        () =>
                        {
                            // re-create the original container so that a brand new IRazorPageActivator
                            // is produced, if we don't re-create the container then it will just return the same instance.
                            ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
                            return recreatedServices.GetRequiredService<IRazorViewEngine>();
                        }, s.GetRequiredService<PureLiveModelFactory>()));

            //builder.Services.AddSingleton<IRazorViewEngine, RefreshingRazorViewEngine>();

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

            builder.Phases.Add(new CustomRazorPhase());

            // NOTE: This is called before all of the default options that are applied
            // in AddRazorRuntimeCompilation, see https://github.com/dotnet/aspnetcore/blob/336e05577cd8bec2000ffcada926189199e4cef0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/DependencyInjection/RazorRuntimeCompilationMvcCoreBuilderExtensions.cs#L88
            // are done so you can't replace anything here that is added by the default razor runtime compilation.
        }
    }

    internal class CustomRazorPhase : RazorEnginePhaseBase, IRazorEnginePhase
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            // it's possible to modify the razor generated document with custom phases.
            // like possibly setting default import statements, etc..
            // there's no documentation on this so you'll need to read the source code to figure
            // that one out if we ever wanted it.
        }
    }

    // We need a custom assembly extension so we can pass state into the initializer :/
    internal class ModelsBuilderAssemblyExtension : AssemblyExtension
    {
        public ModelsBuilderAssemblyExtension(IServiceProvider serviceProvider, string extensionName, Assembly assembly)
            : base(extensionName, assembly) => ServiceProvider = serviceProvider;

        public IServiceProvider ServiceProvider { get; }
    }

    //// The default razor page activator keeps an internal cache of activations, this allows clearning that cache
    //// TODO: Find out if we really need to clear this cache or not? Or if just clearing the view engine cache is enough?
    //internal class RefreshingRazorPageActivator : IRazorPageActivator
    //{
    //    private readonly Func<IRazorPageActivator> _defaultRazorPageActivatorFactory;
    //    private readonly PureLiveModelFactory _pureLiveModelFactory;
    //    private IRazorPageActivator _current;

    //    public RefreshingRazorPageActivator(
    //        Func<IRazorPageActivator> defaultRazorPageActivatorFactory,
    //        PureLiveModelFactory pureLiveModelFactory)
    //    {
    //        _pureLiveModelFactory = pureLiveModelFactory;
    //        _defaultRazorPageActivatorFactory = defaultRazorPageActivatorFactory;
    //        _current = _defaultRazorPageActivatorFactory();
    //        _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
    //    }

    //    // TODO: Do we need to lock?
    //    private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = _defaultRazorPageActivatorFactory();

    //    public void Activate(IRazorPage page, ViewContext context) => _current.Activate(page, context);
    //}

    // We need to have a refreshing razor view engine - the default keeps an in memory cache of views and it cannot be cleared because
    // the cache key instance is internal and would require manually tracking all keys since it cannot be iterated.
    // So like other 'Refreshing' intances, we just create a brand new one and let the old one die therefore clearing the cache.

    // TODO: It looks like dynamic recompile works just fine with "only" the refreshing razor view engine BUT
    // that's also because it creates a new instance of the IRazorPageActivator since when resolving the engine
    // again it's of course going to resolve all dependencies as well which is a few:
    // https://github.com/dotnet/aspnetcore/blob/e37ddbcdbc445a65c6f51549775d5924423880e4/src/Mvc/Mvc.Razor/src/RazorViewEngine.cs#L51
    // which isn't ideal.
    // There's no real way to clear the cache since we cannot iterate and have no access to the cache key instance

    internal class RefreshingRazorViewEngine : /*RazorViewEngine,*/ IRazorViewEngine
    {
        private IRazorViewEngine _current;
        private readonly PureLiveModelFactory _pureLiveModelFactory;
        private readonly Func<IRazorViewEngine> _defaultRazorViewEngineFactory;

        //public RefreshingRazorViewEngine(
        //    PureLiveModelFactory pureLiveModelFactory,
        //    IRazorPageFactoryProvider pageFactory,
        //    IRazorPageActivator pageActivator,
        //    HtmlEncoder htmlEncoder,
        //    IOptions<RazorViewEngineOptions> optionsAccessor,
        //    ILoggerFactory loggerFactory,
        //    DiagnosticListener diagnosticListener)
        //    : base(pageFactory, pageActivator, htmlEncoder, optionsAccessor, loggerFactory, diagnosticListener)
        //{
        //    _pureLiveModelFactory = pureLiveModelFactory;
        //    _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        //}

        public RefreshingRazorViewEngine(Func<IRazorViewEngine> defaultRazorViewEngineFactory, PureLiveModelFactory pureLiveModelFactory)
        {
            _pureLiveModelFactory = pureLiveModelFactory;
            _defaultRazorViewEngineFactory = defaultRazorViewEngineFactory;
            _current = _defaultRazorViewEngineFactory();
            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        // TODO: Do we need to lock?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e)
        {
            //var cache = (Microsoft.Extensions.Caching.Memory.MemoryCache)ViewLookupCache;
            //// clear 100% of the cache.
            //// TODO: This seems to work but need to verify
            //cache.Compact(100);

            _current = _defaultRazorViewEngineFactory();
        }

        public RazorPageResult FindPage(ActionContext context, string pageName) => _current.FindPage(context, pageName);

        public string GetAbsolutePath(string executingFilePath, string pagePath) => _current.GetAbsolutePath(executingFilePath, pagePath);

        public RazorPageResult GetPage(string executingFilePath, string pagePath) => _current.GetPage(executingFilePath, pagePath);

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => _current.FindView(context, viewName, isMainPage);

        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage) => _current.GetView(executingFilePath, viewPath, isMainPage);
    }

    //// The default view compiler creates the compiler once and only once. That compiler will have a stale list of references
    //// to build against so it needs to be re-created. The only way to do that due to internals is to wrap it and re-create
    //// the default instance therefore resetting the compiler references.
    //// TODO: Find out if we really need to clear this cache or not? Or if just clearing the view engine cache is enough?
    //internal class RefreshingRuntimeViewCompilerProvider : IViewCompilerProvider
    //{
    //    private IViewCompilerProvider _current;
    //    private readonly Func<IViewCompilerProvider> _defaultViewCompilerProviderFactory;
    //    private readonly PureLiveModelFactory _pureLiveModelFactory;

    //    public RefreshingRuntimeViewCompilerProvider(
    //        Func<IViewCompilerProvider> defaultViewCompilerProviderFactory,
    //        PureLiveModelFactory pureLiveModelFactory)
    //    {
    //        _defaultViewCompilerProviderFactory = defaultViewCompilerProviderFactory;
    //        _pureLiveModelFactory = pureLiveModelFactory;
    //        _current = _defaultViewCompilerProviderFactory();
    //        _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
    //    }

    //    // TODO: Do we need to lock?
    //    private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _current = _defaultViewCompilerProviderFactory();

    //    public IViewCompiler GetCompiler() => _current.GetCompiler();
    //}

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

        public override RazorCodeDocument Process(RazorProjectItem projectItem) => base.Process(projectItem);

        public override RazorCodeDocument Process(RazorSourceDocument source, string fileKind, IReadOnlyList<RazorSourceDocument> importSources, IReadOnlyList<TagHelperDescriptor> tagHelpers) => base.Process(source, fileKind, importSources, tagHelpers);

        public override RazorCodeDocument ProcessDeclarationOnly(RazorProjectItem projectItem) => base.ProcessDeclarationOnly(projectItem);

        public override RazorCodeDocument ProcessDeclarationOnly(RazorSourceDocument source, string fileKind, IReadOnlyList<RazorSourceDocument> importSources, IReadOnlyList<TagHelperDescriptor> tagHelpers) => base.ProcessDeclarationOnly(source, fileKind, importSources, tagHelpers);

        public override RazorCodeDocument ProcessDesignTime(RazorProjectItem projectItem) => base.ProcessDesignTime(projectItem);

        public override RazorCodeDocument ProcessDesignTime(RazorSourceDocument source, string fileKind, IReadOnlyList<RazorSourceDocument> importSources, IReadOnlyList<TagHelperDescriptor> tagHelpers) => base.ProcessDesignTime(source, fileKind, importSources, tagHelpers);

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
    internal class MetadataReferenceFeatureWrapper : IMetadataReferenceFeature
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
    /// A custom <see cref="IMetadataReferenceFeature"/> that will dynamically resolve a reference for razor (tag helpers) based on the current PureLive assembly.
    /// </summary>
    internal class PureLiveMetadataReferenceFeature : IMetadataReferenceFeature
    {
        /*
         * TODO:
         * This is the only public API available to 'refresh' without hacking is the IMetadataReferenceFeature but
         * it's not clear what this does since it's only used by a single service in aspnetcore.
         * It is not responsible for recompiling views, but is for tag helpers.
         * Used in the CompilationTagHelperFeature which ends up setting the 'Compilation' on the TagDescriptorProviderContext and
         * the result of GetDescriptors() is used in the DefaultRazorTagHelperBinderPhase.
         * This is all about compiling tag helpers. 
         * So am thinking we'll need to 'refresh' this feature as well as the normal razor views.
         */

        private readonly PureLiveModelFactory _pureLiveModelFactory;
        private MetadataReference[] _pureLiveReferences;

        public PureLiveMetadataReferenceFeature(PureLiveModelFactory pureLiveModelFactory)
        {
            _pureLiveModelFactory = pureLiveModelFactory;
            _pureLiveModelFactory.ModelsChanged += PureLiveModelFactory_ModelsChanged;
        }

        /// <inheritdoc/>
        public IReadOnlyList<MetadataReference> References
        {
            get
            {
                // return the reference if we have one
                if (_pureLiveReferences != null)
                {
                    return _pureLiveReferences;
                }

                // else check if we need to create the reference
                if (_pureLiveModelFactory.CurrentModelsMetadataReference != null)
                {
                    _pureLiveReferences = new[] { _pureLiveModelFactory.CurrentModelsMetadataReference };

                    return _pureLiveReferences;
                }

                return Array.Empty<MetadataReference>();
            }
        }

        /// <inheritdoc/>
        public RazorEngine Engine { get; set; }

        /// <summary>
        /// When the models change, clear our references
        /// </summary>
        // TODO: Determine what happens without locking this, will it matter?
        private void PureLiveModelFactory_ModelsChanged(object sender, EventArgs e) => _pureLiveReferences = null;
    }
}
