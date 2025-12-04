using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Default API.
/// </summary>
public class ConfigureDefaultApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureDefaultApiOptions"/> class.
    /// </summary>
    /// <param name="schemaIdSelector">The schema ID selector.</param>
    public ConfigureDefaultApiOptions(ISchemaIdSelector schemaIdSelector)
        : base(schemaIdSelector)
    {
    }

    /// <inheritdoc />
    protected override string ApiName => DefaultApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override string ApiTitle => "Default API";

    /// <inheritdoc />
    protected override string ApiVersion => "Latest";

    /// <inheritdoc />
    protected override string ApiDescription => "All endpoints not defined under specific APIs";
}
