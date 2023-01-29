using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.New.Cms.Core.Models.Configuration;

namespace Umbraco.Cms.Api.Management;

public class ManagementApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // TODO Should just call a single extension method that can be called fromUmbracoTestServerTestBase too, instead of calling this method

        IServiceCollection services = builder.Services;

        builder
            .AddJson()
            .AddNewInstaller()
            .AddUpgrader()
            .AddSearchManagement()
            .AddTrees()
            .AddLanguages()
            .AddDictionary()
            .AddFileUpload()
            .AddHealthCheck()
            .AddModelsBuilder()
            .AddRedirectUrl()
            .AddTrackedReferences()
            .AddDataTypes()
            .AddTemplates()
            .AddLogViewer()
            .AddBackOfficeAuthentication()
            .AddApiVersioning()
            .AddSwaggerGen();

        services
            .ConfigureOptions<ConfigureMvcOptions>()
            .Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "BackOfficeManagementApiFilter",
                    applicationBuilder => applicationBuilder.UseProblemDetailsExceptionHandling(),
                    applicationBuilder => applicationBuilder.UseSwagger(),
                    applicationBuilder => applicationBuilder.UseEndpoints()));
            })
            .AddControllers()
            .AddJsonOptions(_ =>
            {
                // any generic JSON options go here
            })
            .AddJsonOptions(New.Cms.Core.Constants.JsonOptionsNames.BackOffice, options =>
            {
                // all back-office specific JSON options go here
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonObjectConverter());
            });

        // FIXME: when this is moved to core, make the AddUmbracoOptions extension private again and remove core InternalsVisibleTo for Umbraco.Cms.Api.Management
        builder.AddUmbracoOptions<NewBackOfficeSettings>();
        // FIXME: remove this when NewBackOfficeSettings is moved to core
        services.AddSingleton<IValidateOptions<NewBackOfficeSettings>, NewBackOfficeSettingsValidator>();
    }
}

