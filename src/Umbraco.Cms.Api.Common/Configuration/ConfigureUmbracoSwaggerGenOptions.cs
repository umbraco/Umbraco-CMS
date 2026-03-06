using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Configures Swagger/OpenAPI generation options for Umbraco APIs.
/// </summary>
public class ConfigureUmbracoSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IOperationIdSelector _operationIdSelector;
    private readonly ISchemaIdSelector _schemaIdSelector;
    private readonly ISubTypesSelector _subTypesSelector;
    private readonly IDocumentInclusionSelector _documentInclusionSelector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureUmbracoSwaggerGenOptions"/> class.
    /// </summary>
    /// <param name="operationIdSelector">The operation ID selector.</param>
    /// <param name="schemaIdSelector">The schema ID selector.</param>
    /// <param name="subTypesSelector">The sub-types selector for polymorphism support.</param>
    /// <param name="documentInclusionSelector">The document inclusion selector.</param>
    public ConfigureUmbracoSwaggerGenOptions(
        IOperationIdSelector operationIdSelector,
        ISchemaIdSelector schemaIdSelector,
        ISubTypesSelector subTypesSelector,
        IDocumentInclusionSelector documentInclusionSelector)
    {
        _operationIdSelector = operationIdSelector;
        _schemaIdSelector = schemaIdSelector;
        _subTypesSelector = subTypesSelector;
        _documentInclusionSelector = documentInclusionSelector;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureUmbracoSwaggerGenOptions"/> class.
    /// </summary>
    /// <param name="operationIdSelector">The operation ID selector.</param>
    /// <param name="schemaIdSelector">The schema ID selector.</param>
    /// <param name="subTypesSelector">The sub-types selector for polymorphism support.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ConfigureUmbracoSwaggerGenOptions(
        IOperationIdSelector operationIdSelector,
        ISchemaIdSelector schemaIdSelector,
        ISubTypesSelector subTypesSelector)
        : this(
              operationIdSelector,
              schemaIdSelector,
              subTypesSelector,
              StaticServiceProvider.Instance.GetRequiredService<IDocumentInclusionSelector>())
    {
    }

    /// <inheritdoc/>
    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            DefaultApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs",
            });

        swaggerGenOptions.CustomOperationIds(description => _operationIdSelector.OperationId(description));
        swaggerGenOptions.DocInclusionPredicate(_documentInclusionSelector.Include);
        swaggerGenOptions.TagActionsBy(api =>
            api.GroupName is null
                ? []
                : new[] { api.GroupName });
        swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.SchemaFilter<SupportedDerivedTypesSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.SchemaId);
        swaggerGenOptions.SelectSubTypesUsing(_subTypesSelector.SubTypes);
        swaggerGenOptions.SupportNonNullableReferenceTypes();
    }

    /// <summary>
    ///     Generates a sort key for API actions.
    /// </summary>
    /// <param name="apiDesc">The API description.</param>
    /// <returns>A string used to sort API operations in the documentation.</returns>
    /// <remarks>
    ///     See https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting.
    /// </remarks>
    private static string ActionOrderBy(ApiDescription apiDesc)
        => $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{(apiDesc.ActionDescriptor.RouteValues.TryGetValue("action", out var action) ? action : null)}_{apiDesc.HttpMethod}";
}
