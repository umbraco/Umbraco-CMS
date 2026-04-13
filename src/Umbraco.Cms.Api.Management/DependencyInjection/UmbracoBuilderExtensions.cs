using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Registers and configures all services, controllers, and options required for the Umbraco Management API.
    /// This includes endpoints and features for managing Umbraco backoffice resources via HTTP APIs.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to add the Management API services to.</param>
    /// <returns>The <see cref="IUmbracoBuilder"/> instance with Management API services configured.</returns>
    public static IUmbracoBuilder AddUmbracoManagementApi(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;
        builder.Services.AddSingleton<BackOfficeAreaRoutes>();
        builder.Services.AddSingleton<BackOfficeExternalLoginProviderErrorMiddleware>();
        builder.Services.AddSingleton<IManagementApiRouteBuilder, ManagementApiRouteBuilder>();
        builder.Services.AddUnique<IConflictingRouteService, ConflictingRouteService>();
        builder.AddUmbracoApiOpenApiUI();

#pragma warning disable CS0618 // Type or member is obsolete
        if (!services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(JsonPatchService)))
#pragma warning restore CS0618 // Type or member is obsolete
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ModelsBuilderBuilderExtensions.AddModelsBuilder(builder)
                .AddJson()
#pragma warning restore CS0618 // Type or member is obsolete
                .AddInstaller()
                .AddUpgrader()
                .AddSearchManagement()
                .AddTrees()
                .AddAuditLogs()
                .AddConfigurationFactories()
                .AddDocuments()
                .AddDocumentTypes()
                .AddElements()
                .AddMedia()
                .AddMediaTypes()
                .AddMemberGroups()
                .AddMember()
                .AddMemberTypes()
                .AddLanguages()
                .AddDictionary()
                .AddHealthChecks()
                .AddRedirectUrl()
                .AddTags()
                .AddTrackedReferences()
                .AddTemporaryFiles()
                .AddDynamicRoot()
                .AddDataTypes()
                .AddTemplates()
                .AddRelationTypes()
                .AddLogViewer()
                .AddUsers()
                .AddUserGroups()
                .AddPackages()
                .AddManifests()
                .AddEntities()
                .AddScripts()
                .AddPartialViews()
                .AddStylesheets()
                .AddWebhooks()
                .AddServer()
                .AddCorsPolicy()
                .AddPreview()
                .AddServerEvents()
                .AddPasswordConfiguration()
                .AddSupplemenataryLocalizedTextFileSources()
                .AddUserData()
                .AddSegment()
                .AddExport()
                .AddImport()
                .AddNewsDashboard();

            services
                .ConfigureOptions<ConfigureApiBehaviorOptions>()
                .AddControllers()
                .AddJsonOptions(_ =>
                {
                    // any generic JSON options go here
                })
                .AddJsonOptions(Constants.JsonOptionsNames.BackOffice, _ => { });

            builder.Services.AddUmbracoApi<ConfigureUmbracoManagementApiOpenApiOptions>(ManagementApiConfiguration.ApiName, ManagementApiConfiguration.ApiTitle);
            builder.Services.ConfigureOptions<ConfigureUmbracoBackofficeJsonOptions>();

            // Configures the JSON options for the Open API schema generation (based on the back-office MVC JSON options)
            builder.Services.ConfigureOptions<ConfigureUmbracoBackofficeHttpJsonOptions>();

            // Replaces the internal Microsoft OpenApiSchemaService in order to ensure the correct JSON options are used
            builder.Services.ReplaceOpenApiSchemaService();

            services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(
                    new UmbracoPipelineFilter(
                    "BackOfficeManagementApiFilter",
                    applicationBuilder => applicationBuilder.UseProblemDetailsExceptionHandling(),
                    preMapEndpoints: endpoints => endpoints.MapManagementApiEndpoints()));
            });
        }

        builder.AddCollectionBuilders();

        return builder;
    }

    /// <summary>
    /// Replaces the OpenApiSchemaService to use the Management API JSON serializer options, instead of the default http JSON options.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <remarks>This is needed because the OpenAPI schema generation relies on the JSON options to determine how to generate the schemas.
    /// There is a proposal to add support for this currently open: https://github.com/dotnet/aspnetcore/issues/60738.</remarks>
    private static void ReplaceOpenApiSchemaService(this IServiceCollection serviceCollection)
    {
        ServiceDescriptor serviceDescriptor = serviceCollection
            .FirstOrDefault(x => x.ServiceType.Name == "OpenApiSchemaService" && Equals(x.ServiceKey, ManagementApiConfiguration.ApiName))
            ?? throw new InvalidOperationException("Could not find the OpenApiSchemaService when replacing the registered implementation with one created with the management API JSON options.");

        serviceCollection.Remove(serviceDescriptor);
        serviceCollection.Add(
            new ServiceDescriptor(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ServiceKey,
                (sp, serviceKey) => sp.CreateInstance(
                    serviceDescriptor.KeyedImplementationType!,
                    serviceKey!,
                    Options.Create(
                        sp.GetRequiredService<IOptionsMonitor<JsonOptions>>()
                            .Get(Constants.JsonOptionsNames.BackOffice))),
                ServiceLifetime.Singleton));
    }
}
