using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Configures <see cref="ApiBehaviorOptions"/> for Umbraco APIs.
/// </summary>
public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    /// <inheritdoc/>
    public void Configure(ApiBehaviorOptions options) =>
        // disable ProblemDetails as default result type for every non-success response (i.e. 404)
        // - see https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apibehavioroptions.suppressmapclienterrors
        options.SuppressMapClientErrors = true;
}
