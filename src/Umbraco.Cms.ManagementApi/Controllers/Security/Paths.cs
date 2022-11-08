namespace Umbraco.Cms.ManagementApi.Controllers.Security;

public static class Paths
{
    public const string BackOfficeApiEndpointTemplate = "security/back-office";

    public static string BackOfficeApiAuthorizationEndpoint = BackOfficeApiEndpointPath($"{BackOfficeApiEndpointTemplate}/authorize");

    public static string BackOfficeApiTokenEndpoint = BackOfficeApiEndpointPath($"{BackOfficeApiEndpointTemplate}/token");

    private static string BackOfficeApiEndpointPath(string relativePath) => $"/umbraco/management/api/v1.0/{relativePath}";
}
