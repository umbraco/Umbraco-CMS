using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.Web.Common.ModelsBuilder;
using Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

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
 * requirement, we add the MB assembly to the assembly parts manager within the InMemoryModelFactory when the assembly
 * is (re)generated. But due to the above restrictions, when re-generating, this will have no effect since the references
 * have already been resolved with the LazyInitializer in the RazorReferenceManager. There is a known public API
 * where you can add reference paths to the runtime razor compiler via it's IOptions: MvcRazorRuntimeCompilationOptions
 * however this falls short too because those references are just loaded via the RazorReferenceManager and lazy initialized.
 *
 * The services that can be replaced are: IViewCompilerProvider (default is the internal RuntimeViewCompilerProvider) and
 * IViewCompiler (default is the internal RuntimeViewCompiler). There is one specific public extension point that I was
 * hoping would solve all of the problems which was IMetadataReferenceFeature (implemented by LazyMetadataReferenceFeature
 * which uses RazorReferencesManager) which is a razor feature that you can add
 * to the RazorProjectEngine. It is used to resolve roslyn references and by default is backed by RazorReferencesManager.
 * Unfortunately, this service is not used by the CSharpCompiler, it seems to only be used by some tag helper compilations.
 *
 * There are caches at several levels, all of which are not publicly accessible APIs (apart from RazorViewEngine.ViewLookupCache
 * which is possible to clear by casting and then calling cache.Compact(100); but that doesn't get us far enough).
 *
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
 * ... Option C worked, however after a breaking change from dotnet, we cannot go with this options any longer.
 * The reason for this is that when the default RuntimeViewCompiler loads in the assembly using Assembly.Load,
 * This will not work for us since this loads the compiled views into the default AssemblyLoadContext,
 * and our compiled models are loaded in the collectible UmbracoAssemblyLoadContext, and as per the breaking change
 * you're no longer allowed reference a collectible load context from a non-collectible one
 * That is the non-collectible compiled views are not allowed to reference the collectible InMemoryAuto models.
 * https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/7.0/collectible-assemblies
 *
 * So what do we do then?
 * We've had to go with option a unfortunately, and we've cloned the above classes
 * There has had to be some modifications to the ViewCompiler (CollectibleRuntimeViewCompiler)
 * First off we've added a new class InMemoryAssemblyLoadContextManager, the role of this class is to ensure that
 * no one will take a reference to the assembly load context (you cannot unload an assembly load context if there's any references to it).
 * This means that both the InMemoryAutoFactory and the ViewCompiler uses the LoadContextManager to load their assemblies.
 * This serves another purpose being that it keeps track of the location of the models assembly.
 * This means that we no longer use the RazorReferencesManager to resolve that specific dependency, but instead add and explicit dependency to the models assembly.
 *
 * With this our assembly load context issue is solved, however the caching issue still persists now that we no longer use the RefreshingRazorViewEngine
 * To clear these caches another class the RuntimeCompilationCacheBuster has been introduced,
 * this keeps a reference to the CollectibleRuntimeViewCompiler and the RazorViewEngine and is injected into the InMemoryModelsFactory to clear the caches when rebuilding modes.
 * In order to avoid having to copy all the RazorViewEngine code the cache buster uses reflection to call the internal ClearCache method of the RazorViewEngine.
 */

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the common Umbraco functionality
/// </summary>
public static class UmbracoBuilderDependencyInjectionExtensions
{
    /// <summary>
    ///     Adds umbraco's embedded model builder support
    /// </summary>
    public static IUmbracoBuilder AddModelsBuilder(this IUmbracoBuilder builder)
    {
        var umbServices = new UniqueServiceDescriptor(typeof(UmbracoServices), typeof(UmbracoServices), ServiceLifetime.Singleton);
        if (builder.Services.Contains(umbServices))
        {
            // if this ext method is called more than once just exit
            return builder;
        }

        builder.Services.Add(umbServices);

        if (builder.Config.GetRuntimeMode() == RuntimeMode.BackofficeDevelopment)
        {
            // Configure services to allow InMemoryAuto mode
            builder.AddInMemoryModelsRazorEngine();

            builder.AddNotificationHandler<ModelBindingErrorNotification, ModelsBuilderNotificationHandler>();
        }

        if (builder.Config.GetRuntimeMode() != RuntimeMode.Production)
        {
            // Configure service to allow models generation
            builder.AddNotificationHandler<ServerVariablesParsingNotification, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<TemplateSavingNotification, ModelsBuilderNotificationHandler>();

            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<UmbracoRequestEndNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, AutoModelsNotificationHandler>();
            
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, OutOfDateModelsStatus>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, OutOfDateModelsStatus>();
        }

        builder.Services.TryAddSingleton<IModelsBuilderDashboardProvider, NoopModelsBuilderDashboardProvider>();

        // Register required services for ModelsBuilderDashboardController
        builder.Services.AddSingleton<IModelsGenerator, ModelsGenerator>();
        
        // TODO: Remove in v13 - this is only here in case someone is already using this generator directly
        builder.Services.AddSingleton<ModelsGenerator>();
        builder.Services.AddSingleton<OutOfDateModelsStatus>();
        builder.Services.AddSingleton<ModelsGenerationError>();

        return builder;
    }

    // See notes in RefreshingRazorViewEngine for information on what this is doing.
    private static IUmbracoBuilder AddInMemoryModelsRazorEngine(this IUmbracoBuilder builder)
    {
        // We should only add/replace these services when models builder is InMemory, otherwise we'll cause issues.
        // Since these services expect the ModelsMode to be InMemoryAuto
        if (builder.Config.GetModelsMode() is ModelsMode.InMemoryAuto)
        {
            builder.Services.AddSingleton<UmbracoRazorReferenceManager>();
            builder.Services.AddSingleton<CompilationOptionsProvider>();
            builder.Services.AddSingleton<IViewCompilerProvider, UmbracoViewCompilerProvider>();
            builder.Services.AddSingleton<RuntimeCompilationCacheBuster>();
            builder.Services.AddSingleton<InMemoryAssemblyLoadContextManager>();

            builder.Services.AddSingleton<InMemoryModelFactory>();
            // Register the factory as IPublishedModelFactory
            builder.Services.AddSingleton<IPublishedModelFactory, InMemoryModelFactory>();
            return builder;
        }

        // This is what the community MB would replace, all of the above services are fine to be registered
        builder.Services.AddSingleton<IPublishedModelFactory>(factory => factory.CreateDefaultPublishedModelFactory());
        
        return builder;
    }
}
