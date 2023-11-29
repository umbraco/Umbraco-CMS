using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions options) =>
        // disable ProblemDetails as default result type for every non-success response (i.e. 404)
        // - see https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apibehavioroptions.suppressmapclienterrors
        options.SuppressMapClientErrors = true;
}
