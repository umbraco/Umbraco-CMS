using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureApiVersioningOptions : IConfigureOptions<ApiVersioningOptions>
{
    public void Configure(ApiVersioningOptions options)
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        options.AssumeDefaultVersionWhenUnspecified = true; // This is required for the old backoffice to work
        options.UseApiBehavior = false;
    }
}
