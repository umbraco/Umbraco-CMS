using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Configuration;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoManagementApi(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        if (!services.Any(x => x.ImplementationType == typeof(JsonPatchService)))
        {
            ModelsBuilderBuilderExtensions.AddModelsBuilder(builder)
                .AddJson()
                .AddNewInstaller()
                .AddUpgrader()
                .AddSearchManagement()
                .AddTrees()
                .AddAuditLogs()
                .AddDocuments()
                .AddDocumentTypes()
                .AddMedia()
                .AddMediaTypes()
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
                .AddPathFolders()
                .AddScripts()
                .AddPartialViews()
                .AddStylesheets()
                .AddServer()
                .AddCorsPolicy()
                .AddBackOfficeAuthentication();

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

            // FIXME: when this is moved to core, make the AddUmbracoOptions extension private again and remove core InternalsVisibleTo for Umbraco.Cms.Api.Management
            builder.AddUmbracoOptions<NewBackOfficeSettings>();
            // FIXME: remove this when NewBackOfficeSettings is moved to core
            services.AddSingleton<IValidateOptions<NewBackOfficeSettings>, NewBackOfficeSettingsValidator>();
        }

        return builder;
    }
}
