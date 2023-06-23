namespace Umbraco.Cms.Api.Management.Controllers.Security;

public static class Paths
{
    public const string BackOfficeApiEndpointTemplate = "security/back-office";

    public static readonly string BackOfficeApiAuthorizationEndpoint = BackOfficeApiEndpointPath($"{BackOfficeApiEndpointTemplate}/authorize");

    public static readonly string BackOfficeApiTokenEndpoint = BackOfficeApiEndpointPath($"{BackOfficeApiEndpointTemplate}/token");

    private static string BackOfficeApiEndpointPath(string relativePath) => $"/umbraco/management/api/v1.0/{relativePath}";
}
