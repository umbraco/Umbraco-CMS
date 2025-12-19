namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Default API.
/// </summary>
public class ConfigureDefaultApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <inheritdoc />
    protected override string ApiName => DefaultApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override string ApiTitle => "Default API";

    /// <inheritdoc />
    protected override string ApiVersion => "Latest";

    /// <inheritdoc />
    protected override string ApiDescription => "All endpoints not defined under specific APIs";
}
