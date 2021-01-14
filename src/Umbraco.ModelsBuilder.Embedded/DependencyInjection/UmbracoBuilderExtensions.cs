using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;

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
            builder.Services.AddSingleton<ModelsBuilderNotificationHandler>();
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
            // that is nornally created and use that to create our custom one while ensuring all of the razor features
            // that we can't really add ourselves are there.
            // Pretty much all methods, even thnigs like SetCSharpLanguageVersion are actually adding razor features.

            //var internalServicesBuilder = new ServiceCollection();
            //internalServicesBuilder.AddControllersWithViews().AddRazorRuntimeCompilation();
            //var internalServices = internalServicesBuilder.BuildServiceProvider();
            //var defaultRazorProjectEngine = internalServices.GetRequiredService<RazorProjectEngine>();

            ServiceProvider internalServices = builder.Services.BuildServiceProvider();
            RazorProjectEngine defaultRazorProjectEngine = internalServices.GetRequiredService<RazorProjectEngine>();

            builder.Services.AddSingleton(s =>
            {
                RazorProjectFileSystem fileSystem = s.GetRequiredService<RazorProjectFileSystem>();

                // Create the project engine
                var projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, builder =>
                {
                    // replace all features with the defaults
                    builder.Features.Clear();

                    foreach (IRazorEngineFeature f in defaultRazorProjectEngine.EngineFeatures)
                    {
                        builder.Features.Add(f);
                    }

                    foreach (IRazorProjectEngineFeature f in defaultRazorProjectEngine.ProjectFeatures)
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
                    metadataReferenceFeatures.Add(new PureLiveMetadataReferenceFeature(s.GetRequiredService<PureLiveModelFactory>()));

                    // now add them to our wrapper and back into the features
                    builder.Features.Add(new MetadataReferenceFeatureWrapper(metadataReferenceFeatures));

                    //RazorExtensions.Register(builder);

                    //// Roslyn + TagHelpers infrastructure
                    //// TODO: These are internal...
                    //var referenceManager = s.GetRequiredService<RazorReferenceManager>();
                    //builder.Features.Add(new LazyMetadataReferenceFeature(referenceManager));

                    //builder.Features.Add(new CompilationTagHelperFeature());

                    //// TagHelperDescriptorProviders (actually do tag helper discovery)
                    //builder.Features.Add(new DefaultTagHelperDescriptorProvider());
                    //builder.Features.Add(new ViewComponentTagHelperDescriptorProvider());
                    //builder.SetCSharpLanguageVersion(csharpCompiler.ParseOptions.LanguageVersion);
                });

                return projectEngine;
            });

            return builder;
        }
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
