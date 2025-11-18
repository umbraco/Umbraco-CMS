using System.Text.Json.Serialization.Metadata;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
/// Base class for configuring OpenAPI options for Umbraco APIs.
/// </summary>
public abstract class ConfigureUmbracoOpenApiOptionsBase : IConfigureNamedOptions<OpenApiOptions>
{
    /// <summary>
    ///  Gets the name/identifier of the API to configure.
    /// </summary>
    protected abstract string ApiName { get; }

    /// <summary>
    ///  Gets the name/identifier of the API to configure.
    /// </summary>
    protected abstract string ApiTitle { get; }

    /// <summary>
    ///  Gets the version of the API to configure.
    /// </summary>
    protected abstract string ApiVersion { get; }

    /// <summary>
    ///  Gets the description of the API to configure.
    /// </summary>
    protected abstract string ApiDescription { get; }

    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        // No default configuration
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        if (name != ApiName)
        {
            return;
        }

        ConfigureOpenApi(options);
    }

    /// <summary>
    /// Configure the OpenAPI options for the specified API.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    protected virtual void ConfigureOpenApi(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = ApiTitle,
                Version = ApiVersion,
                Description = ApiDescription,
            };
            return Task.CompletedTask;
        });

        options.ShouldInclude = ShouldInclude;
        options.CreateSchemaReferenceId = CreateSchemaReferenceId;

        options.AddOperationTransformer<CustomOperationIdsTransformer>();

        // Tag actions by group name and cleanup unused tags (caused by the tag changes)
        options
            .AddOperationTransformer<TagActionsByGroupNameTransformer>()
            .AddDocumentTransformer<TagActionsByGroupNameTransformer>()
            .AddDocumentTransformer<SortTagsAndPathsTransformer>();

        options.AddSchemaTransformer<RequireNonNullablePropertiesSchemaTransformer>();
        options.AddSchemaTransformer<FixFileReturnTypesTransformer>();
    }

    private static string? CreateSchemaReferenceId(JsonTypeInfo jsonTypeInfo)
    {
        // Ensure that only types that would normally be included in the schema generation are given a schema reference ID.
        // Otherwise, we should return null to inline them.
        var defaultSchemaReferenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        if (defaultSchemaReferenceId is null)
        {
            return null;
        }

        return jsonTypeInfo.Type.Namespace?.StartsWith("Umbraco.Cms") == true
            ? UmbracoSchemaIdGenerator.Generate(jsonTypeInfo.Type)
            : defaultSchemaReferenceId;
    }

    private bool ShouldInclude(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor && controllerActionDescriptor.HasMapToApiAttribute(ApiName))
        {
            return true;
        }

        ApiVersionMetadata apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        return apiVersionMetadata.Name == ApiName
               || (string.IsNullOrEmpty(apiVersionMetadata.Name) && ApiName == DefaultApiConfiguration.ApiName);
    }
}
