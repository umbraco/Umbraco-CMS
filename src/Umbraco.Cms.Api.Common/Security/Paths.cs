namespace Umbraco.Cms.Api.Common.Security;

public static class Paths
{
    public static class BackOfficeApi
    {
        public const string EndpointTemplate = "security/back-office";

        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        // TODO: talk to Jacob about what is the preferred versioning scheme for the new backoffice client - /v1.0/ or /v1/
        private static string EndpointPath(string relativePath) => $"/umbraco/management/api/v1.0/{relativePath}";
    }

    public static class MemberApi
    {
        public const string EndpointTemplate = "security/member";

        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        public static readonly string LogoutEndpoint = EndpointPath($"{EndpointTemplate}/signout");

        public static readonly string RevokeEndpoint = EndpointPath($"{EndpointTemplate}/revoke");

        // NOTE: we're NOT using /api/v1.0/ here because it will clash with the Delivery API docs
        private static string EndpointPath(string relativePath) => $"/umbraco/delivery/api/v1/{relativePath}";
    }
}
