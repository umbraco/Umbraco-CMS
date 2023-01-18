using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ManagementApiConfiguration
{
    internal const string ApiTitle = "Umbraco Backoffice API";

    internal const string DefaultApiDocumentName = "v1";

    internal static ApiVersion DefaultApiVersion => new(1, 0);
}
