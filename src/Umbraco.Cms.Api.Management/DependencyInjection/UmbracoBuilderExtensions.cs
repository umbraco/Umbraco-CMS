﻿using Microsoft.Extensions.DependencyInjection;
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
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

public static partial class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoManagementApi(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;
        builder.Services.AddSingleton<BackOfficeAreaRoutes>();
        builder.Services.AddSingleton<BackOfficeExternalLoginProviderErrorMiddleware>();
        builder.Services.AddUnique<IConflictingRouteService, ConflictingRouteService>();

        if (!services.Any(x => x.ImplementationType == typeof(JsonPatchService)))
        {
            ModelsBuilderBuilderExtensions.AddModelsBuilder(builder)
                .AddJson()
                .AddInstaller()
                .AddUpgrader()
                .AddSearchManagement()
                .AddTrees()
                .AddAuditLogs()
                .AddDocuments()
                .AddDocumentTypes()
                .AddMedia()
                .AddMediaTypes()
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
                .AddTours()
                .AddPackages()
                .AddEntities()
                .AddScripts()
                .AddPartialViews()
                .AddStylesheets()
                .AddWebhooks()
                .AddServer()
                .AddCorsPolicy()
                .AddWebhooks()
                .AddPreview()
                .AddPasswordConfiguration()
                .AddSupplemenataryLocalizedTextFileSources();

            services
                .ConfigureOptions<ConfigureApiBehaviorOptions>()
                .AddControllers()
                .AddJsonOptions(_ =>
                {
                    // any generic JSON options go here
                })
                .AddJsonOptions(Constants.JsonOptionsNames.BackOffice, _ => { });

            services.ConfigureOptions<ConfigureUmbracoBackofficeJsonOptions>();
            services.ConfigureOptions<ConfigureUmbracoManagementApiSwaggerGenOptions>();

            services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "BackOfficeManagementApiFilter",
                    applicationBuilder => applicationBuilder.UseProblemDetailsExceptionHandling(),
                    applicationBuilder => { },
                    applicationBuilder => applicationBuilder.UseEndpoints()));
            });
        }

        return builder;
    }
}
