using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
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

        options.ShouldInclude = ShouldInclude;
        options.CreateSchemaReferenceId = type => OpenApiOptions.CreateDefaultSchemaReferenceId(type) is null ? null : UmbracoSchemaIdGenerator.Generate(type);

        options.AddOperationTransformer<CustomOperationIdsTransformer>();

        // Tag actions by group name and cleanup unused tags (caused by the tag changes)
        options
            .AddOperationTransformer<TagActionsByGroupNameTransformer>()
            .AddDocumentTransformer<TagActionsByGroupNameTransformer>()
            .AddDocumentTransformer<SortTagsAndPathsTransformer>();

        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddSchemaTransformer<RequireNonNullablePropertiesSchemaTransformer>();

        ConfigureOpenApi(options);

        // swaggerGenOptions.SelectSubTypesUsing(_subTypesSelector.SubTypes);
        // swaggerGenOptions.SupportNonNullableReferenceTypes();
    }

    /// <summary>
    /// Configure the OpenAPI options for the specified API.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    protected abstract void ConfigureOpenApi(OpenApiOptions options);

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
