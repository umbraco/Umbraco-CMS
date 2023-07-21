namespace Umbraco.Cms.Api.Common.Security;

public static class Paths
{
    public static class BackOfficeApi
    {
        public const string EndpointTemplate = "security/back-office";

        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        private static string EndpointPath(string relativePath) => $"/umbraco/management/api/v1.0/{relativePath}";
    }

    public static class MemberApi
    {
        public const string EndpointTemplate = "security/member";

        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        public static readonly string LogoutEndpoint = EndpointPath($"{EndpointTemplate}/logout");

        private static string EndpointPath(string relativePath) => $"/umbraco/delivery/api/v1.0/{relativePath}";
    }
}
