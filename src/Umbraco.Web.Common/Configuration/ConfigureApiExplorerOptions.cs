using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Common.Configuration;

public sealed class ConfigureApiExplorerOptions : IConfigureOptions<ApiExplorerOptions>
{
    private readonly IOptions<ApiVersioningOptions> _apiVersioningOptions;

    public ConfigureApiExplorerOptions(IOptions<ApiVersioningOptions> apiVersioningOptions)
    {
        _apiVersioningOptions = apiVersioningOptions;
    }

    public void Configure(ApiExplorerOptions options)
    {
        options.DefaultApiVersion = _apiVersioningOptions.Value.DefaultApiVersion;
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
        options.AddApiVersionParametersWhenVersionNeutral = true;
        options.AssumeDefaultVersionWhenUnspecified = false;
    }
}
