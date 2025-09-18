using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;
using Umbraco.Extensions;

namespace Umbraco.Cms.DevelopmentMode.Backoffice.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddBackofficeDevelopment(this IUmbracoBuilder builder)
    {
        if (builder.Config.GetRuntimeMode() != RuntimeMode.BackofficeDevelopment)
        {
            return builder;
        }

        builder.AddMvcAndRazor(mvcBuilder =>
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        });
        builder.AddInMemoryModelsRazorEngine();
        builder.RuntimeModeValidators()
            .Add<InMemoryModelsBuilderModeValidator>();
        builder.AddNotificationHandler<ModelBindingErrorNotification, ModelsBuilderBindingErrorHandler>();

        return builder;
    }


    // See notes in RefreshingRazorViewEngine for information on what this is doing.
    private static IUmbracoBuilder AddInMemoryModelsRazorEngine(this IUmbracoBuilder builder)
    {
        // We should only add/replace these services when models builder is InMemory, otherwise we'll cause issues.
        // Since these services expect the ModelsMode to be InMemoryAuto
        if (builder.Config.GetModelsMode() == ModelsModeConstants.InMemoryAuto)
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
