using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureUmbracoSwaggerGenOptions : IConfigureNamedOptions<OpenApiOptions>
{
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
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs",
            };
            return Task.CompletedTask;
        });

        // swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        // swaggerGenOptions.SelectSubTypesUsing(_subTypesSelector.SubTypes);
        // swaggerGenOptions.SupportNonNullableReferenceTypes();
    }
}
