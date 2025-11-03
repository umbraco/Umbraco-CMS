using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureUmbracoSwaggerGenOptions : IConfigureNamedOptions<OpenApiOptions>
{
    //private readonly IOperationIdSelector _operationIdSelector;
    //private readonly ISchemaIdSelector _schemaIdSelector;
    //private readonly ISubTypesSelector _subTypesSelector;

    public ConfigureUmbracoSwaggerGenOptions(
        IOperationIdSelector operationIdSelector,
        ISchemaIdSelector schemaIdSelector,
        ISubTypesSelector subTypesSelector)
    {
        //_operationIdSelector = operationIdSelector;
        //_schemaIdSelector = schemaIdSelector;
        //_subTypesSelector = subTypesSelector;
    }

    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        // No default configuration
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        if (name != DefaultApiConfiguration.ApiName)
        {
            return;
        }

        options.ConfigureUmbracoDefaultApiOptions(name);
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs",
            };
            return Task.CompletedTask;
        });

        // swaggerGenOptions.CustomOperationIds(description => _operationIdSelector.OperationId(description));
        // swaggerGenOptions.DocInclusionPredicate((name, api) =>
        // {
        //     if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
        //         && controllerActionDescriptor.HasMapToApiAttribute(name))
        //     {
        //         return true;
        //     }
        //
        //     ApiVersionMetadata apiVersionMetadata = api.ActionDescriptor.GetApiVersionMetadata();
        //     return apiVersionMetadata.Name == name
        //            || (string.IsNullOrEmpty(apiVersionMetadata.Name) && name == DefaultApiConfiguration.ApiName);
        // });
        // swaggerGenOptions.TagActionsBy(api => new[] { api.GroupName });
        // swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        // swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        // swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.SchemaId);
        // swaggerGenOptions.SelectSubTypesUsing(_subTypesSelector.SubTypes);
        // swaggerGenOptions.SupportNonNullableReferenceTypes();
    }
}
